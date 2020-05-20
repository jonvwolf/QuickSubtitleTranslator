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
        /// Main Entry
        /// </summary>
        /// <param name="path">Path where subtitles files are</param>
        /// <param name="outputFolder">Folder to save translated files</param>
        /// <param name="fromLang">Source language</param>
        /// <param name="toLang">Translate to</param>
        /// <param name="api">Which service provider to use</param>
        /// <param name="apiKey">API key for service provider</param>
        /// <param name="askForRetry">If enabled, it will wait for input if http/service fails before continuing</param>
        /// <param name="charLimit">Will stop processing files if it reachers the character limit</param>
        public static void Main(string path, string outputFolder, string fromLang, string toLang, ApiType api, string apiKey, bool askForRetry = false, long charLimit = 0)
        {
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
            Console.WriteLine($"Ask for retry = {askForRetry}");
            Console.WriteLine($"Max characters to send = {charLimit}");
            Console.WriteLine($"Default encoding is UTF-8");
            Console.WriteLine($"Default framerate for microdvd subtitles is {25}");

            new App().Run(path, outputFolder, fromLang, toLang, api, apiKey, askForRetry, charLimit);
        }

    }
}
