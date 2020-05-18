using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace QuickSubtitleTranslator.IntegrationTestsMockEx
{
    public static class Helper
    {
        public const string SrtExampleFile = "srt example.srt";
        public const string ScrubsFile = "Scrubs.S02E08.srt";
        public const string SrtExampleFileNF = "srt example_nf.srt";
        public const string ScrubsFileNF = "Scrubs.S02E08_nf.srt";

        public const string FromLang = "en";
        public const string ToLang = "es";
        public const string Path = @"..\..\..\..\test_folder_inputs";
        public const string ValidatePath = @"..\..\..\..\mockex_validate_files";

        public static void CheckDirectory(string outputFolder)
        {
            if (Directory.Exists(outputFolder))
            {
                File.Delete($"{outputFolder}\\{ScrubsFile}");
                File.Delete($"{outputFolder}\\{SrtExampleFile}");

                File.Delete($"{outputFolder}\\{ScrubsFileNF}");
                File.Delete($"{outputFolder}\\{SrtExampleFileNF}");
            }
        }

        public static void AssertFilesAreIdentical(string file1, string file2)
        {
            byte[] md5Source;
            byte[] md5Output;
            using (var md5 = MD5.Create())
            {
                using var stream = File.OpenRead(file1);
                md5Source = md5.ComputeHash(stream);
            }
            using (var md5 = MD5.Create())
            {
                using var stream = File.OpenRead(file2);
                md5Output = md5.ComputeHash(stream);
            }

            Assert.True(md5Source.SequenceEqual(md5Output), $"{file1} is not identical as {file2}");
        }

        public static void AssertMockExFilesHaveStrings(string srt, string scrubs)
        {
            string contents = File.ReadAllText(srt, Encoding.UTF8);
            Assert.Contains("00:00:01,600 --> 00:00:04,200", contents);
            Assert.Contains("English (US)", contents);
            Assert.Contains("Adding Iñtërnâtiônàlizætiøn will it work? Hola", contents);
            Assert.Contains("- I would have been called. - Stop making excuses. 💝🐹🌇⛔", contents);

            string contents2 = File.ReadAllText(scrubs, Encoding.UTF8);
            Assert.Contains("00:00:01,360 --> 00:00:04,079", contents2);
            Assert.Contains("00:20:12,880 --> 00:20:16,919", contents2);
            Assert.Contains("00:03:41,120 --> 00:03:45,398", contents2);
            Assert.Contains("<i>As a doctor, you spend</i> <i>about a third of your nights</i>", contents2);
            Assert.Contains("<i>Or you can steal stuff</i> <i>from the hospital.</i>", contents2);
            Assert.Contains("- on the nursing-home patients. - Well, what can I say?", contents2);
        }
    }
}
