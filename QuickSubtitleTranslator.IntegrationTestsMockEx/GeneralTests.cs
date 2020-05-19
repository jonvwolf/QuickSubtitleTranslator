using Moq;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            if (!File.Exists("accepted_notice"))
                File.WriteAllText("accepted_notice", "Yes");
        }

        [Fact]
        public void OutputSubtitle_ShouldBe_Exact()
        {
            /*
             * --path "..\..\..\..\test_folder" --output-folder "sub_output" --from-lang "en" --to-lang "es" --api "Google"
             */
            string path = @"..\..\..\..\mockex_test_OutputSubtitle_ShouldBe_Exact";
            string outputFolder = "sub_output";
            string fromLang = "en";
            string toLang = "es";
            string apiKey = "";
            ApiType api = ApiType.Google;

            if (Directory.Exists("sub_output"))
            {
                File.Delete("sub_output\\Scrubs.S02E08.srt");
                File.Delete("sub_output\\srt example.srt");
                File.Delete("sub_output\\sub_example.srt");
            }

            var svc = new Mock<ITranslationService>();
            svc.Setup(x => x.Translate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<MySubtitleItem>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns((string to, string from, IReadOnlyList<MySubtitleItem> list, string key, bool waitForInput) =>
                {
                    return new MyTranslateResult(list.ToImmutableList(), 0);
                });

            var app = new App()
            {
                TranslationService = svc.Object,
                //Dirty fix so it doesn't break up lines
            };
            app.Run(path, outputFolder, fromLang, toLang, api, apiKey);

            Helper.AssertFilesAreIdentical(path + @"\" + "srt example.srt", @"sub_output\" + "srt example.srt");
            Helper.AssertFilesAreIdentical($@"{path}\Scrubs.S02E08.srt", @"sub_output\Scrubs.S02E08.srt");
            Helper.AssertFilesAreIdentical($@"{path}\sub_example.sub", @"sub_output\sub_example.srt");
        }
    }
}
