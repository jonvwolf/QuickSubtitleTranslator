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
        
        public const string FromLang = "en";
        public const string ToLang = "es";
        public const string Path = @"..\..\..\..\test_folder_inputs";
        public const string ValidatePath = Path;

        public static void CheckDirectory(string outputFolder)
        {
            if (Directory.Exists(outputFolder))
            {
                File.Delete($"{outputFolder}\\{ScrubsFile}");
                File.Delete($"{outputFolder}\\{SrtExampleFile}");

                File.Delete($"{outputFolder}\\mockex_01The.King.Of.Queens.S05E01.srt");
                File.Delete($"{outputFolder}\\mockex_01the.king.of.queens.s08e01.srt");
                File.Delete($"{outputFolder}\\mockex_02The.King.Of.Queens.S09E02.srt");
                File.Delete($"{outputFolder}\\mockex_30RockS01E01.srt");
                File.Delete($"{outputFolder}\\mockex_30RockS07E12E13.srt");
                File.Delete($"{outputFolder}\\mockex_E08The Yoga Bear.srt");
                File.Delete($"{outputFolder}\\mockex_Evangelion 2.22.srt");
                File.Delete($"{outputFolder}\\mockex_Malcolm in the Middle - 111.srt");
                File.Delete($"{outputFolder}\\mockex_MasterChef.US.S06E08.srt");
                File.Delete($"{outputFolder}\\mockex_The_IT_Crowd_0103.srt");
            }
        }

        public static void ValidateAllFiles(string outputFolder)
        {
            AssertMockExFilesHaveStrings($@"{outputFolder}\{SrtExampleFile}", $@"{outputFolder}\{ScrubsFile}");
            AssertFilesAreIdentical($"{ValidatePath}\\{ScrubsFile}", $"{outputFolder}\\{ScrubsFile}");
            AssertFilesAreIdentical($"{ValidatePath}\\{SrtExampleFile}", $"{outputFolder}\\{SrtExampleFile}");

            AssertFilesAreIdentical($"{ValidatePath}\\mockex_01The.King.Of.Queens.S05E01.srt", $"{outputFolder}\\mockex_01The.King.Of.Queens.S05E01.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_01the.king.of.queens.s08e01.srt", $"{outputFolder}\\mockex_01the.king.of.queens.s08e01.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_02The.King.Of.Queens.S09E02.srt", $"{outputFolder}\\mockex_02The.King.Of.Queens.S09E02.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_30RockS01E01.srt", $"{outputFolder}\\mockex_30RockS01E01.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_30RockS07E12E13.srt", $"{outputFolder}\\mockex_30RockS07E12E13.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_E08The Yoga Bear.srt", $"{outputFolder}\\mockex_E08The Yoga Bear.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_Evangelion 2.22.srt", $"{outputFolder}\\mockex_Evangelion 2.22.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_Malcolm in the Middle - 111.srt", $"{outputFolder}\\mockex_Malcolm in the Middle - 111.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_MasterChef.US.S06E08.srt", $"{outputFolder}\\mockex_MasterChef.US.S06E08.srt");
            AssertFilesAreIdentical($"{ValidatePath}\\mockex_The_IT_Crowd_0103.srt", $"{outputFolder}\\mockex_The_IT_Crowd_0103.srt");
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
            Assert.Contains("Adding Iñtërnâtiônàlizætiøn will it work?\r\nHola", contents);
            Assert.Contains("- I would have been called.\r\n- Stop making excuses. 💝🐹🌇⛔", contents);

            string contents2 = File.ReadAllText(scrubs, Encoding.UTF8);
            Assert.Contains("00:00:01,360 --> 00:00:04,079", contents2);
            Assert.Contains("00:20:12,880 --> 00:20:16,919", contents2);
            Assert.Contains("00:03:41,120 --> 00:03:45,398", contents2);
            Assert.Contains("<i>As a doctor, you spend</i>\r\n<i>about a third of your nights</i>", contents2);
            Assert.Contains("<i>Or you can steal stuff</i>\r\n<i>from the hospital.</i>", contents2);
            Assert.Contains("- on the nursing-home patients.\r\n- Well, what can I say?", contents2);
        }
    }
}
