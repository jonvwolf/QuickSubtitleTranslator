using QuickSubtitleTranslator.Common;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace QuickSubtitleTranslator
{
    /// <summary>
    /// Quick subtitle translator Program class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Service providers for translation
        /// </summary>
        public enum APIType
        {
            /// <summary>
            /// Google API
            /// </summary>
            Google, 
            /// <summary>
            /// Microsoft API
            /// </summary>
            Microsoft,
            /// <summary>
            /// IBM API
            /// </summary>
            IBM,
            /// <summary>
            /// Amazon API
            /// </summary>
            Amazon,
        }
        /// <summary>
        /// This property helps to override the default service providers
        /// </summary>
        public static ITranslationService TranslationService { get; set; }
        static void ShowNotice()
        {
            if (!File.Exists("accepted_notice"))
            {
                Console.WriteLine("WARNING! Read please");
                Console.WriteLine("This application is under MIT license. Please read LICENSE.txt file");
                Console.WriteLine("Using a service provider may incur charges to your billing account.");
                Console.WriteLine("Even though, Google and Microsoft/etc. offer free X amount of translated characters, you must monitor and set up resource limits in your Google/Microsoft/etc. account");
                Console.WriteLine("Make sure you understand their billing and set up proper budget alerts/API resource caps and limits so you don't get unexpected charges");
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
        
        static void SetService(APIType api)
        {
            if (TranslationService == null)
            {
                if (api == APIType.Google)
                {
                    TranslationService = new GoogleApi.GoogleTranslator();
                }
                else if (api == APIType.Microsoft)
                {
                    TranslationService = new MicrosoftApi.MicrosoftTranslator();
                }
                else if (api == APIType.Amazon)
                {
                    TranslationService = new AmazonApi.AmazonTranslator();
                }
                else if (api == APIType.IBM)
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
        /// Main Entry
        /// </summary>
        /// <param name="path">Path where subtitles files are</param>
        /// <param name="outputFolder">Folder to save translated files</param>
        /// <param name="fromLang">Source language</param>
        /// <param name="toLang">Translate to</param>
        /// <param name="api">Which service provider to use</param>
        /// <param name="apiKey">API key for service provider</param>
        /// <param name="overwrite">Overwrite output subtitles folder</param>
        public static void Main(string path, string outputFolder, string fromLang, string toLang, APIType api, string apiKey, bool overwrite = true)
        {
            ShowNotice();

            Console.WriteLine("README.md has some information on how to use this app. Any bugs raise a bug in github");
            Console.WriteLine("Version v2-alpha");
            Console.WriteLine("Github url: https://github.com/jonwolfdev/QuickSubtitleTranslator");
            Console.WriteLine("License: MIT License (read License.txt file)");
            Console.WriteLine("This app contains work from (read NOTICE.txt file): https://github.com/AlexPoint/SubtitlesParser");
            Console.WriteLine();

            Console.WriteLine($"Path = {path}");
            Console.WriteLine($"Output = {outputFolder}");
            Console.WriteLine($"From language = {fromLang}");
            Console.WriteLine($"To language = {toLang}");
            Console.WriteLine($"API = {api}");
            Console.WriteLine($"Overwrite = {overwrite}");
            Console.WriteLine($"Default encoding is UTF-8");
            Console.WriteLine($"Default framerate for microdvd subtitles is {25}");

            CreateAndCheckOutputFolder(outputFolder);

            SetService(api);

            var files = GetFiles(path);
            int totalCharacters = 0;
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

                if (myItems.Count == 0)
                    throw new Exception($"File {file} has no subtitles...");

                Console.WriteLine($"File {file} has {myItems.Count} subtitle items");

                var translatedItems = TranslationService.Translate(fromLang, toLang, myItems, apiKey);
                Console.WriteLine($"Translated count for {file}. Count: {translatedItems.TranslatedCharacters}");

                ValidateItems(items, translatedItems.TranslatedItems.ToImmutableList());
                WriteToFile(translatedItems.TranslatedItems.ToImmutableList(), file, outputFolder);

                totalCharacters += translatedItems.TranslatedCharacters;
            }

            Console.WriteLine($"Total characters used: {totalCharacters}");
        }

        static void ValidateItems(IReadOnlyList<SubtitleItem> original, IReadOnlyList<MySubtitleItem> translated)
        {
            if (original.Count != translated.Count)
                throw new Exception($"Items do not match between {original} and {translated}");

            for (int index = 0; index < original.Count; index++)
            {
                if (original[index].EndTime != translated[index].EndTime || original[index].StartTime != translated[index].StartTime)
                {
                    throw new Exception("Difference between original and translated");
                }
            }
        }

        static void WriteToFile(IReadOnlyList<MySubtitleItem> items, string outputFile, string outputFolder)
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

            sb.Remove(sb.Length - (Environment.NewLine.Length * 2), (Environment.NewLine.Length * 2));

            string newFileName = Path.GetFileNameWithoutExtension(outputFile);
            string fullPathNewFile = Path.Combine(outputFolder, newFileName + ".srt");

            Console.WriteLine($"Writing to a new SRT file... {fullPathNewFile}");
            
            File.WriteAllText(fullPathNewFile, sb.ToString());
        }
    }
}
