using QuickSubtitleTranslator.Common;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public enum API
        {
            /// <summary>
            /// Google API
            /// </summary>
            Google, 
            /// <summary>
            /// Microsoft API
            /// </summary>
            Microsoft
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
                Console.WriteLine("Even though, Google and Microsoft offer free X amount of translated characters, you must monitor and set up a limit in your Google/Microsoft account");
                Console.WriteLine("Make sure you understand their billing and set up proper budget limits so you don't get unexpected charges");
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
        /// <summary>
        /// Main Entry
        /// </summary>
        /// <param name="path">Path where subtitles files are</param>
        /// <param name="outputFolder">Folder to save translated files</param>
        /// <param name="fromLang">Source language</param>
        /// <param name="toLang">Translate to</param>
        /// <param name="api">Which service provider to use</param>
        /// <param name="overwrite">Overwrite output subtitles folder</param>
        public static void Main(string path, string outputFolder, string fromLang, string toLang, API api, bool overwrite = true)
        {
            ShowNotice();
            
            Console.WriteLine($"Path = {path}");
            Console.WriteLine($"Output = {outputFolder}");
            Console.WriteLine($"From language = {fromLang}");
            Console.WriteLine($"To language = {toLang}");
            Console.WriteLine($"API = {api.ToString()}");
            Console.WriteLine($"Overwrite = {overwrite}");
            Console.WriteLine($"Default encoding is UTF-8");
            Console.WriteLine($"Default framerate for microdvd subtitles is {25}");

            CreateAndCheckOutputFolder(outputFolder);

            if (TranslationService == null)
            {
                if (api == API.Google)
                {
                    TranslationService = new GoogleApi.GoogleTranslator();
                }
                else if (api == API.Microsoft)
                {
                    TranslationService = new MicrosoftApi.MicrosoftTranslator();
                }
                else
                {
                    throw new InvalidOperationException($"Invalid API type: {api.ToString()}");
                }
            }

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

                        //var translatedItems = TranslationService.Translate(fromLang, toLang, );
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine($"WARNING! Invalid subtitle file [{file}] Skipping file. Error message: {ex.Message}");
                    }
                }
            }

        }
    }
}
