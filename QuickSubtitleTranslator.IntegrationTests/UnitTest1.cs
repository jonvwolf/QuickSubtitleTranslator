using System;
using System.IO;
using Xunit;
using static QuickSubtitleTranslator.Program;

namespace QuickSubtitleTranslator.IntegrationTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            /*
             * --path "..\..\..\..\test_folder" --output-folder "sub_output" --from-lang "en" --to-lang "es" --api "Google"
             */
            string path = @"..\..\..\..\test_folder";
            string outputFolder = "sub_output";
            string fromLang = "en";
            string toLang = "es";
            API api = API.Google;

            File.WriteAllText("accepted_notice", "Yes");
            Program.Main(path, outputFolder, fromLang, toLang, api);
        }
    }
}
