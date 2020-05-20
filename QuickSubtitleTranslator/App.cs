using QuickSubtitleTranslator.Common;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QuickSubtitleTranslator
{
    /// <summary>
    /// Main logic for translation
    /// </summary>
    public class App
    {
        /// <summary>
        /// This property helps to override the default service providers
        /// </summary>
        public ITranslationService TranslationService { get; set; }
        void SetService(ApiType api)
        {
            if (TranslationService == null)
            {
                if (api == ApiType.Google)
                {
                    TranslationService = new GoogleApi.GoogleTranslator();
                }
                else if (api == ApiType.Microsoft)
                {
                    TranslationService = new MicrosoftApi.MicrosoftTranslator();
                }
                else if (api == ApiType.Amazon)
                {
                    TranslationService = new AmazonApi.AmazonTranslator();
                }
                else if (api == ApiType.IBM)
                {
                    TranslationService = new IBMApi.IBMTranslator();
                }
                else
                {
                    throw new InvalidOperationException($"Invalid API type: {api}");
                }
            }
        }

        /// <summary>
        /// Process all files
        /// </summary>
        public void Run(string path, string outputFolder, string fromLang, string toLang, ApiType api, string apiKey, bool askForRetry = false, long maxCharactersToSend = 0, bool peek = false)
        {
            Console.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
            ShowNotice();
            CreateAndCheckOutputFolder(outputFolder);
            SetService(api);

            Console.WriteLine();

            var files = GetFiles(path).ToList();
            int totalCharacters = 0;
            int currentFileCount = 0;
            
            long remainingCharactersToSend = maxCharactersToSend;
            foreach (var file in files)
            {
                var parser = new SubtitlesParser.Classes.Parsers.SubParser();

                List<MySubtitleItem> myItems = new List<MySubtitleItem>();
                List<SubtitleItem> items = new List<SubtitleItem>();
                using (var stream = File.OpenRead(file))
                {
                    try
                    {
                        items = parser.ParseStream(stream);
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine($"WARNING! Invalid subtitle file [{file}] Skipping file. Error message: {ex.Message}");
                        continue;
                    }

                    myItems.Capacity = items.Count;
                    foreach (var item in items)
                    {
                        myItems.Add(item.CopyToMySubtitleItem());
                    }
                }

                foreach (var item in myItems)
                {
                    if (item.EndTime <= 0 || item.StartTime <= 0 || item.Lines.Count <= 0)
                        throw new Exception($"Invalid subtitle file: {file}");

                    foreach (var line in item.Lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            throw new Exception($"Empty line detected in {file}");
                    }
                }

                if (myItems.Count == 0)
                    throw new Exception($"File {file} has no subtitles...");

                currentFileCount++;
                Console.WriteLine($"[START] File {file} has {myItems.Count} subtitle items");
                Console.WriteLine($"Processing file {currentFileCount} out of {files.Count}");

                var translatedItems = TranslationService.Translate(fromLang, toLang, myItems, apiKey, askForRetry, remainingCharactersToSend, peek);
                if (translatedItems.SkippedBecauseOfCharacterLimit)
                {
                    Console.WriteLine($"Character limit reached. Skipping current and remaining files...");
                    break;
                }

                ValidateItems(items, translatedItems.TranslatedItems.ToImmutableList());
                WriteToFile(translatedItems.TranslatedItems.ToImmutableList(), file, outputFolder);

                totalCharacters += translatedItems.TranslatedCharacters;
                remainingCharactersToSend -= translatedItems.TranslatedCharacters;

                Console.WriteLine();
                Console.WriteLine($"Remaining characters to send: {remainingCharactersToSend}");
                Console.WriteLine($"Characters translated (of file): {translatedItems.TranslatedCharacters}");
                Console.WriteLine($"Current total characters sent: {totalCharacters}");
                Console.WriteLine($"[FINISH] Finished file: {file}");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Completed all files.");
            Console.WriteLine($"Remaining characters (unused): {remainingCharactersToSend}");
            Console.WriteLine($"Total characters used: {totalCharacters}");
            Console.WriteLine($"Char limit: {maxCharactersToSend}");
            Console.WriteLine($"Char limit - total characters used = {maxCharactersToSend - totalCharacters}");
        }
        static void ShowNotice()
        {
            if (!File.Exists("accepted_notice"))
            {
                Console.WriteLine("WARNING! Read please");
                Console.WriteLine("This application is under MIT license. Please read LICENSE.txt file");
                Console.WriteLine("Using a service provider may incur charges to your billing account.");
                Console.WriteLine("Even though, Google and Microsoft/etc. offer free X amount of translated characters, you must monitor and set up resource limits in your Google/Microsoft/etc. account");
                Console.WriteLine("Make sure you understand their billing and set up proper budget alerts so you don't get unexpected charges");
                Console.WriteLine("Do you understand this is your own responsability? [Yes] [No]");
                string answer = Console.ReadLine();
                if (answer.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllText("accepted_notice", answer);
                }
                else
                {
                    Console.WriteLine("Exitting...");
                    Environment.Exit(0);
                }
            }
        }
        static void CreateAndCheckOutputFolder(string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            var folder = File.GetAttributes(outputFolder);
            if (!folder.HasFlag(FileAttributes.Directory))
            {
                throw new InvalidOperationException("Output folder param must indicate a folder in order to write translated SRT files");
            }

            var files = Directory.GetFiles(outputFolder, "*");
            var dirs = Directory.GetDirectories(outputFolder, "*");

            if (files.Length > 0)
                throw new InvalidOperationException("Output folder already contains files");

            if (dirs.Length > 0)
                throw new InvalidOperationException("Output folder already contains folders");
        }
        static IEnumerable<string> GetFiles(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x =>
                    x.EndsWith(".srt", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".sub", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".ssa", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".ttml", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".vtt", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            if (!files.Any())
            {
                throw new InvalidOperationException("No subtitles found. (.srt, .sub, .ssa, .ttml, .vtt, .xml)");
            }
            return files;
        }

        static void ValidateItems(IReadOnlyList<SubtitleItem> original, IReadOnlyList<MySubtitleItem> translated)
        {
            if (original.Count != translated.Count)
                throw new Exception($"Items do not match between {original} and {translated}");

            for (int index = 0; index < original.Count; index++)
            {
                if (original[index].EndTime != translated[index].EndTime || original[index].StartTime != translated[index].StartTime)
                    throw new Exception("Difference between original and translated start and end times");

                if (translated[index].Lines.Count != original[index].Lines.Count)
                    throw new Exception("Translated lines and original lines count do not match");

                foreach (var line in translated[index].Lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        throw new Exception($"There should be not an empty string");
                }
            }

        }

        void WriteToFile(IReadOnlyList<MySubtitleItem> items, string outputFile, string outputFolder)
        {
            //* 80 is an estimate
            StringBuilder sb = new StringBuilder(items.Count * 80);

            for (int i = 0; i < items.Count; i++)
            {
                string format = "{0:00}:{1:00}:{2:00},{3:000}";
                TimeSpan start = TimeSpan.FromMilliseconds(items[i].StartTime);
                TimeSpan end = TimeSpan.FromMilliseconds(items[i].EndTime);

                sb.Append(i + 1).Append(Environment.NewLine)
                    .Append(string.Format(CultureInfo.InvariantCulture, format, start.Hours, start.Minutes, start.Seconds, start.Milliseconds))
                    .Append(" --> ")
                    .Append(string.Format(CultureInfo.InvariantCulture, format, end.Hours, end.Minutes, end.Seconds, end.Milliseconds))
                    .Append(Environment.NewLine);

                foreach (var line in items[i].Lines)
                {
                    sb.Append(line)
                        .Append(Environment.NewLine);
                }

                sb.Append(Environment.NewLine);
            }

            string newFileName = Path.GetFileNameWithoutExtension(outputFile);
            string fullPathNewFile = Path.Combine(outputFolder, newFileName + ".srt");

            Console.WriteLine($"Writing to a new SRT file... {fullPathNewFile}");

            File.WriteAllText(fullPathNewFile, sb.ToString());
        }
    }
}
