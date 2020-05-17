using Moq;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Xunit;
using static QuickSubtitleTranslator.Program;

namespace QuickSubtitleTranslator.IntegrationTestsMockEx
{
    public class GeneralTests
    {
        public GeneralTests()
        {
            File.WriteAllText("accepted_notice", "Yes");
        }

        [Fact]
        public void OutputSubtitle_ShouldBe_Exact()
        {
            /*
             * --path "..\..\..\..\test_folder" --output-folder "sub_output" --from-lang "en" --to-lang "es" --api "Google"
             */
            string path = @"..\..\..\..\test_folder";
            string outputFolder = "sub_output";
            string fromLang = "en";
            string toLang = "es";
            string apiKey = "";
            APIType api = APIType.Google;

            if (Directory.Exists("sub_output"))
            {
                File.Delete("sub_output\\Scrubs.S02E08.srt");
                File.Delete("sub_output\\srt example.srt");
                File.Delete("sub_output\\sub_example.srt");
            }

            //Dirty fix so it doesn't break up lines
            Constants.DoNotBreakIfOnlyLine = 1000;
            Constants.MaxWordsPerLine = 1000;

            var svc = new Mock<ITranslationService>();
            svc.Setup(x => x.Translate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<MySubtitleItem>>(), It.IsAny<string>()))
                .Returns((string to, string from, IReadOnlyList<MySubtitleItem> list, string key) =>
                {
                    var listx = new List<MyTranslatedSubtitleItem>(list.Count);
                    foreach (var item in list)
                    {
                        listx.Add(new MyTranslatedSubtitleItem(item.StartTime, item.EndTime, string.Join("\r\n", item.Lines)));
                    }
                    return new MyTranslateResult(listx, 0);
                });

            Program.TranslationService = svc.Object;
            Program.Main(path, outputFolder, fromLang, toLang, api, apiKey);

            byte[] md5Source;
            byte[] md5Output;
            using (var md5 = MD5.Create())
            {
                using var stream = File.OpenRead(path + @"\" + "srt example.srt");
                md5Source = md5.ComputeHash(stream);
            }
            using (var md5 = MD5.Create())
            {
                using var stream = File.OpenRead(@"sub_output\" + "srt example.srt");
                md5Output = md5.ComputeHash(stream);
            }

            Assert.True(md5Source.SequenceEqual(md5Output), "Files are not identical");
        }
    }
}
