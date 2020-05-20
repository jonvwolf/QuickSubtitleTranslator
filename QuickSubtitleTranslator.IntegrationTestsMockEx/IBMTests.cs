using IBM.Cloud.SDK.Core.Http;
using IBM.Watson.LanguageTranslator.v3.Model;
using QuickSubtitleTranslator.IBMApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace QuickSubtitleTranslator.IntegrationTestsMockEx
{
    public class IBMTests
    {
        const string OutputFolder = "mockex_output_ibm";
        const string ApiKey = "apikey||1||https://api.us-south.language-translator.watson.cloud.ibm.com/instances/1000-001-10001-abc";
        static readonly ApiType Api = ApiType.IBM;

        public IBMTests()
        {
            if (!File.Exists("accepted_notice"))
                File.WriteAllText("accepted_notice", "Yes");
        }

        [Fact]
        public void TestMockEx_Translate_FilesShouldBeIdentical()
        {
            //Arrange
            Helper.CheckDirectory(OutputFolder);

            IBMTranslator.MaxTries = 1;
            IBMTranslator.SleepBetweenCalls = 0;
            IBMTranslator.SleepIfFails = 0;

            IBMTranslator.SendData = (service, subset, modelstr) =>
            {
                var list = new List<Translation>();

                foreach (var item in subset)
                {
                    list.Add(new Translation()
                    {
                        _Translation = item
                    });
                }

                var result = new TranslationResult()
                {
                    Translations = list
                };
                return new DetailedResponse<TranslationResult>()
                {
                    Result = result
                };
            };
            //Act
            Program.Main(Helper.Path, OutputFolder, Helper.FromLang, Helper.ToLang, Api, ApiKey, peek: true);

            //Assert
            Helper.ValidateAllFiles(OutputFolder);
        }
    }
}
