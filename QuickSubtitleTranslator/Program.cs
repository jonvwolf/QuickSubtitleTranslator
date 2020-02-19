using QuickSubtitleTranslator.Common;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
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
                    throw new InvalidOperationException($"Invalid API type: {api.ToString()}");
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
            Console.WriteLine("Version v1-alpha");
            Console.WriteLine("Github url: https://github.com/jonwolfdev/QuickSubtitleTranslator");
            Console.WriteLine("License: MIT License (read License.txt file)");
            Console.WriteLine("This app contains work from (read NOTICE.txt file): https://github.com/AlexPoint/SubtitlesParser");
            Console.WriteLine();

            Console.WriteLine($"Path = {path}");
            Console.WriteLine($"Output = {outputFolder}");
            Console.WriteLine($"From language = {fromLang}");
            Console.WriteLine($"To language = {toLang}");
            Console.WriteLine($"API = {api.ToString()}");
            Console.WriteLine($"Overwrite = {overwrite}");
            Console.WriteLine($"Default encoding is UTF-8");
            Console.WriteLine($"Default framerate for microdvd subtitles is {25}");

            CreateAndCheckOutputFolder(outputFolder);

            SetService(api);

            int totalCharacters = 0;
            var files = GetFiles(path);
            foreach (var file in files)
            {
                var parser = new SubtitlesParser.Classes.Parsers.SubParser();
                using (var stream = File.OpenRead(file))
                {
                    List<SubtitleItem> items;
                    try
                    {
                        Console.WriteLine($"Parsing file: {file}");
                        items = parser.ParseStream(stream);

                        if (items.Count == 0)
                        {
                            Console.WriteLine("No lines parsed... skipping...");
                            continue;
                        }

                        List<string> lines = new List<string>((int)(items.Count * 1.5));
                        int totalLines = 0;
                        int fileCharacters = 0;
                        for (int i = 0; i < items.Count; i++)
                        {
                            for (int lineIndex = 0; lineIndex < items[i].Lines.Count; lineIndex++)
                            {
                                lines.Add(items[i].Lines[lineIndex]);
                                fileCharacters += new StringInfo(items[i].Lines[lineIndex]).LengthInTextElements;
                                totalLines++;
                            }
                        }

                        if (totalLines == 0)
                        {
                            Console.WriteLine("No lines found... skipping...");
                            continue;
                        }

                        Console.WriteLine($"Total characters to send: {fileCharacters}");
                        Console.WriteLine("Calling service provider...");
                        var translatedItems = TranslationService.Translate(fromLang, toLang, lines, apiKey);

                        Console.WriteLine($"Source text lines: {totalLines}");
                        Console.WriteLine($"Translated text lines: {translatedItems.Count}");
                        if (translatedItems.Count != totalLines)
                        {
                            Console.WriteLine("WARNING! Total lines do not match from original subtitle file and service provider response...");
                        }

                        int translatedItemsIndex = 0;
                        StringBuilder sb = new StringBuilder(fileCharacters);
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


                            for (int lineIndex = 0; lineIndex < items[i].Lines.Count; lineIndex++)
                            {
                                string newVal = string.Empty;
                                try
                                {
                                    newVal = translatedItems[translatedItemsIndex];
                                }
                                catch (Exception ex) when (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
                                {
                                    Console.WriteLine($"Since total lines do not match... Error message: {ex.Message}");
                                }
                                items[i].Lines[lineIndex] = newVal;

                                sb.Append(items[i].Lines[lineIndex])
                                    .Append(Environment.NewLine);

                                translatedItemsIndex++;
                            }

                            sb.Append(Environment.NewLine);
                        }

                        sb.Remove(sb.Length - (Environment.NewLine.Length * 2), (Environment.NewLine.Length * 2));

                        totalCharacters += fileCharacters;

                        string newFileName = Path.GetFileNameWithoutExtension(file);
                        string fullPathNewFile = Path.Combine(outputFolder, newFileName + ".srt");

                        Console.WriteLine($"Writing to a new SRT file... {fullPathNewFile}");
                        File.WriteAllText(fullPathNewFile, sb.ToString());
                        Console.WriteLine("End");
                        Console.WriteLine("=============================================================");
                        Console.WriteLine();
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine($"WARNING! Invalid subtitle file [{file}] Skipping file. Error message: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Total characters used: {totalCharacters}");
        }
    }
}
