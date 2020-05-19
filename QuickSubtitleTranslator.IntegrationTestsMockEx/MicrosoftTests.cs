using Newtonsoft.Json;
using QuickSubtitleTranslator.MicrosoftApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;
using static QuickSubtitleTranslator.Program;

namespace QuickSubtitleTranslator.IntegrationTestsMockEx
{
    public class MicrosoftTests
    {
        const string OutputFolder = "mockex_output_microsoft";   
        const string ApiKey = "MyApiKey";
        static readonly ApiType Api = ApiType.Microsoft;

        public MicrosoftTests()
        {
            if (!File.Exists("accepted_notice"))
                File.WriteAllText("accepted_notice", "Yes");
        }

        [Fact]
        public void TestMockEx_Translate_FilesShouldBeIdentical()
        {
            //Arrange
            Helper.CheckDirectory(OutputFolder);

            MicrosoftTranslator.MaxTries = 1;
            MicrosoftTranslator.SleepBetweenCalls = 0;
            MicrosoftTranslator.SleepIfFails = 0;

            MicrosoftTranslator.SendData = (client, request) =>
            {
                var data = new List<TranslationResult>();
                var requestData = JsonConvert.DeserializeObject<IList<dynamic>>(request.Content.ReadAsStringAsync().Result);

                foreach (var item in requestData)
                {
                    var list = new List<Translation>()
                    {
                        new Translation(){Text = item.Text}
                    };

                    data.Add(new TranslationResult()
                    {
                        Translations = list.ToArray()
                    });
                }

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = content
                };
            };
            //Act
            Program.Main(Helper.Path, OutputFolder, Helper.FromLang, Helper.ToLang, Api, ApiKey);

            //Assert
            Helper.ValidateAllFiles(OutputFolder);
        }
    }
}
