using Moq;
using QuickSubtitleTranslator.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Xunit;
using static QuickSubtitleTranslator.Program;
using System.Text;

namespace QuickSubtitleTranslator.IntegrationTests
{
    public class IntegrationTests
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
            string apiKey = "";
            APIType api = APIType.Google;

            File.WriteAllText("accepted_notice", "Yes");

            var svc = new Mock<ITranslationService>();
            svc.Setup(x => x.Translate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns((string to, string from, IList<string> list, string key) =>
                {
                    return list;
                });

            Program.TranslationService = svc.Object;
            Program.Main(path, outputFolder, fromLang, toLang, api, apiKey);

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

            Assert.True(md5Source.SequenceEqual(md5Output), "Files are not identical");
        }

        [Fact]
        public void OutputSubtitle_ShouldBe_TranslatedBy_Google()
        {
            string path = @"..\..\..\..\test_folder_google";
            string outputFolder = "sub_output_translate_google";
            string fromLang = "en";
            string toLang = "es";

            //qsubtranslator_google_key env key
            //format of: qsubtranslator_google_key
            //file#Full_path_to_api_key_file
            //value#api_key
            string apiKey;
            string env = Environment.GetEnvironmentVariable("qsubtranslator_google_key");
            string[] parts = env.Split("#");
            if (parts[0].Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                apiKey = File.ReadAllText(parts[1]);
            }
            else
            {
                apiKey = parts[1];
            }
            APIType api = APIType.Google;

            File.WriteAllText("accepted_notice", "Yes");

            Program.Main(path, outputFolder, fromLang, toLang, api, apiKey);

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

            string file = @"sub_output_translate_google\" + "srt example.srt";
            string contents = File.ReadAllText(file, Encoding.UTF8);
            Assert.Contains("Ingl�s", contents);
            Assert.Contains("00:00:01,600 --> 00:00:04,200", contents);
            Assert.Contains("I�t�rn�ti�n�liz�ti�n", contents);
            Assert.Contains("excusas", contents);

            string file2 = @"sub_output_translate_google\" + "Scrubs.S02E08.srt";
            string contents2 = File.ReadAllText(file2, Encoding.UTF8);
            Assert.Contains("00:00:01,360 --> 00:00:04,079", contents2);
            Assert.Contains("bien", contents2);
            Assert.Contains("00:12:33,720 --> 00:12:36,473", contents2);
            Assert.Contains("ambulancia", contents2);
            Assert.Contains("00:20:21,920 --> 00:20:24,354", contents2);
            Assert.Contains("lugar.", contents2);
        }
    }
}