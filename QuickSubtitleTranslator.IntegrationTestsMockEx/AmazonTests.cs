using Amazon.Translate.Model;
using QuickSubtitleTranslator.AmazonApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace QuickSubtitleTranslator.IntegrationTestsMockEx
{
    public class AmazonTests
    {
        const string OutputFolder = "mockex_output_amazon";
        const string ApiKey = "apikey||1||accesskey||1||us-east-2";
        static readonly ApiType Api = ApiType.Amazon;

        public AmazonTests()
        {
            if (!File.Exists("accepted_notice"))
                File.WriteAllText("accepted_notice", "Yes");
        }

        [Fact]
        public void TestMockEx_Translate_FilesShouldBeIdentical()
        {
            //Arrange
            Helper.CheckDirectory(OutputFolder);

            AmazonTranslator.MaxTries = 1;
            AmazonTranslator.SleepBetweenCalls = 0;
            AmazonTranslator.SleepIfFails = 0;

            AmazonTranslator.SendData = (client, textRequest) =>
            {
                return new TranslateTextResponse()
                {
                    TranslatedText = textRequest.Text
                };
            };
            //Act
            Program.Main(Helper.Path, OutputFolder, Helper.FromLang, Helper.ToLang, Api, ApiKey, peek: true);

            //Assert
            Helper.ValidateAllFiles(OutputFolder);
        }
    }
}
