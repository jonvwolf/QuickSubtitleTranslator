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
using System.Collections.Immutable;

namespace QuickSubtitleTranslator.IntegrationTests
{
    public class IntegrationTests
    {
        const string Path = @"..\..\..\..\test_folder_inputs";
        const string FromLang = "en";
        const string ToLang = "es";
        const string SrtExampleFile = "srt example.srt";
        const string ScrubsFile = "Scrubs.S02E08.srt";
        
        static string GetApiKey(ApiType api)
        {
            //qsubtranslator_google_key env key
            //format of: qsubtranslator_google_key
            //file#Full_path_to_api_key_file
            //value#api_key
            string apiKey;
            string env;

            if (api == ApiType.Google)
            {
                env = Environment.GetEnvironmentVariable("qsubtranslator_google_key");
            }
            else if (api == ApiType.Microsoft)
            {
                env = Environment.GetEnvironmentVariable("qsubtranslator_microsoft_key");
            }
            else if (api == ApiType.IBM)
            {
                env = Environment.GetEnvironmentVariable("qsubtranslator_ibm_key");
            }
            else if (api == ApiType.Amazon)
            {
                env = Environment.GetEnvironmentVariable("qsubtranslator_amazon_key");
            }
            else { throw new InvalidOperationException("Unkown api type: " + api.ToString()); }

            if (string.IsNullOrEmpty(env))
            {
                throw new InvalidOperationException("Environment key is not set.");
            }

            string[] parts = env.Split("#");
            if (parts[0].Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                apiKey = File.ReadAllText(parts[1]);
            }
            else
            {
                apiKey = parts[1];
            }
            return apiKey;
        }
        static void CheckDirectory(string outputFolder)
        {
            if (Directory.Exists(outputFolder))
            {
                File.Delete($"{outputFolder}\\{ScrubsFile}");
                File.Delete($"{outputFolder}\\{SrtExampleFile}");
            }
        }
        public IntegrationTests()
        {
            if (!File.Exists("accepted_notice"))
                File.WriteAllText("accepted_notice", "Yes");
        }

        [Fact]
        public void OutputSubtitle_ShouldBe_TranslatedBy_Google()
        {
            string outputFolder = "sub_output_translate_google";
            ApiType api = ApiType.Google;

            CheckDirectory(outputFolder);

            Program.Main(Path, outputFolder, FromLang, ToLang, api, GetApiKey(api), charLimit: 1000000, peek: true);

            string contents = File.ReadAllText(@$"{outputFolder}\{SrtExampleFile}", Encoding.UTF8);
            Assert.Contains("Inglés", contents);
            Assert.Contains("00:00:01,600 --> 00:00:04,200", contents);
            Assert.Contains("Iñtërnâtiônàlizætiøn", contents);
            Assert.Contains("excusas", contents);

            string contents2 = File.ReadAllText($@"{outputFolder}\{ScrubsFile}", Encoding.UTF8);
            Assert.Contains("00:00:01,360 --> 00:00:04,079", contents2);
            Assert.Contains("00:20:12,880 --> 00:20:16,919", contents2);
            Assert.Contains("00:03:41,120 --> 00:03:45,398", contents2);

            Assert.Contains("bien", contents2);
            Assert.Contains("ambulancia", contents2);
            Assert.Contains("lugar.", contents2);
        }

        [Fact]
        public void OutputSubtitle_ShouldBe_TranslatedBy_Microsoft()
        {
            string outputFolder = "sub_output_translate_microsoft";
            ApiType api = ApiType.Microsoft;

            CheckDirectory(outputFolder);
            Program.Main(Path, outputFolder, FromLang, ToLang, api, GetApiKey(api), charLimit: 1000000, peek: true);

            string contents = File.ReadAllText($@"{outputFolder}\{SrtExampleFile}", Encoding.UTF8);
            Assert.Contains("Inglés", contents);
            Assert.Contains("00:00:01,600 --> 00:00:04,200", contents);
            Assert.Contains("subtítulos", contents);
            Assert.Contains("excusas", contents);

            string contents2 = File.ReadAllText($@"{outputFolder}\{ScrubsFile}", Encoding.UTF8);
            Assert.Contains("00:00:01,360 --> 00:00:04,079", contents2);
            Assert.Contains("00:20:12,880 --> 00:20:16,919", contents2);
            Assert.Contains("00:03:41,120 --> 00:03:45,398", contents2);

            Assert.Contains("bien", contents2);
            Assert.Contains("ambulancia", contents2);
            Assert.Contains("lugar.", contents2);
        }

        [Fact]
        public void OutputSubtitle_ShouldBe_TranslatedBy_Amazon()
        {
            string outputFolder = "sub_output_translate_amazon";
            ApiType api = ApiType.Amazon;
            CheckDirectory(outputFolder);

            Program.Main(Path, outputFolder, FromLang, ToLang, api, GetApiKey(api), charLimit: 1000000, peek: true);

            string contents = File.ReadAllText($@"{outputFolder}\{SrtExampleFile}", Encoding.UTF8);
            Assert.Contains("Añadir", contents);
            Assert.Contains("00:00:01,600 --> 00:00:04,200", contents);
            Assert.Contains("subtítulos", contents);
            Assert.Contains("excusas", contents);

            string contents2 = File.ReadAllText($@"{outputFolder}\{ScrubsFile}", Encoding.UTF8);
        
            Assert.Contains("bien", contents2);
            Assert.Contains("ambulancia", contents2);
            Assert.Contains("lugar.", contents2);

            Assert.Contains("00:00:01,360 --> 00:00:04,079", contents2);
            Assert.Contains("00:20:12,880 --> 00:20:16,919", contents2);
            Assert.Contains("00:03:41,120 --> 00:03:45,398", contents2);
        }

        [Fact]
        public void OutputSubtitle_ShouldBe_TranslatedBy_IBM()
        {
            string outputFolder = "sub_output_translate_ibm";
            ApiType api = ApiType.IBM;

            CheckDirectory(outputFolder);
            Program.Main(Path, outputFolder, FromLang, ToLang, api, GetApiKey(api), charLimit: 1000000, peek: true);

            
            string contents = File.ReadAllText($@"{outputFolder}\{SrtExampleFile}", Encoding.UTF8);
            Assert.Contains("Añadir", contents);
            Assert.Contains("00:00:01,600 --> 00:00:04,200", contents);
            Assert.Contains("subtítulos", contents);
            Assert.Contains("excusas", contents);

            string contents2 = File.ReadAllText($@"{outputFolder}\{ScrubsFile}", Encoding.UTF8);

            Assert.Contains("00:00:01,360 --> 00:00:04,079", contents2);
            Assert.Contains("00:20:12,880 --> 00:20:16,919", contents2);
            Assert.Contains("00:03:41,120 --> 00:03:45,398", contents2);

            Assert.Contains("bien", contents2);
            Assert.Contains("ambulancia", contents2);
            Assert.Contains("lugar.", contents2);
        }
    }
}
