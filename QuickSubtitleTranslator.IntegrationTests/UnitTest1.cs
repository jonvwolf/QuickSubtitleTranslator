using Moq;
using QuickSubtitleTranslator.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Xunit;
using static QuickSubtitleTranslator.Program;

namespace QuickSubtitleTranslator.IntegrationTests
{
    public class UnitTest1
    {
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
            APIType api = APIType.Google;

            File.WriteAllText("accepted_notice", "Yes");

            var svc = new Mock<ITranslationService>();
            svc.Setup(x => x.Translate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns((string to, string from, IList<string> list) =>
                {
                    return list;
                });

            Program.TranslationService = svc.Object;
            Program.Main(path, outputFolder, fromLang, toLang, api);

            byte[] md5Source;
            byte[] md5Output;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path + @"\" + "srt example.srt"))
                {
                    md5Source = md5.ComputeHash(stream);
                }
            }
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(@"sub_output\" + "srt example.srt"))
                {
                    md5Output = md5.ComputeHash(stream);
                }
            }

            Assert.True(md5Source.SequenceEqual(md5Output));
        }
    }
}
