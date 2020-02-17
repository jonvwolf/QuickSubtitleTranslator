using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuickSubtitleTranslator
{
    class Program
    {
        public enum API
        {
            Google, Microsoft
        }
        static void Main(string path, string outputFolder, string fromLang, string toLang, API api, bool overwrite = true, int? frameRateForMicroDvd = null)
        {
            Console.WriteLine($"Path = {path}");
            Console.WriteLine($"Output = {outputFolder}");
            Console.WriteLine($"From language = {fromLang}");
            Console.WriteLine($"To language = {toLang}");
            Console.WriteLine($"API = {api.ToString()}");
            Console.WriteLine($"Overwrite = {overwrite}");
            Console.WriteLine($"FrameRateForMicroDvd = {frameRateForMicroDvd}");

            Directory.CreateDirectory(outputFolder);
            var folder = File.GetAttributes(outputFolder);

            if (!folder.HasFlag(FileAttributes.Directory))
            {
                throw new InvalidOperationException("Output folder param must indicate a folder in order to write translated SRT files");
            }

            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x =>
                    x.EndsWith(".srt", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".sub", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".ssa", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".ttml", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".vtt", StringComparison.OrdinalIgnoreCase) ||
                    x.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            if(!files.Any())
            {
                throw new InvalidOperationException("No subtitles found. (.srt, .sub, .ssa, .ttml, .vtt, .xml)");
            }

            foreach(var file in files)
            {
                var parser = new SubtitlesParser.Classes.Parsers.SubParser();
                using (var stream = File.OpenRead(file))
                {
                    List<SubtitleItem> items;
                    try
                    {
                        items = parser.ParseStream(stream);
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
