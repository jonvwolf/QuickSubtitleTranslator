using Google.Cloud.Translation.V2;
using QuickSubtitleTranslator.GoogleApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using static QuickSubtitleTranslator.Program;

namespace QuickSubtitleTranslator.IntegrationTestsMockEx
{
    public class GoogleTests
    {
        const string OutputFolder = "mockex_output_google";
        const string ApiKey = "MyApiKey";
        static readonly ApiType Api = ApiType.Google;

        public GoogleTests()
        {
            if (!File.Exists("accepted_notice"))
                File.WriteAllText("accepted_notice", "Yes");
        }

        [Fact]
        public void TestMockEx_Translate_FilesShouldBeIdentical()
        {
            //Arrange
            Helper.CheckDirectory(OutputFolder);

            GoogleTranslator.MaxTries = 1;
            GoogleTranslator.SleepBetweenCalls = 0;
            GoogleTranslator.SleepIfFails = 0;

            GoogleTranslator.SendData = (client, subset, from, to, model) =>
            {
                var list = new List<TranslationResult>();
                foreach (var item in subset)
                {
                    list.Add(new TranslationResult(item, item, from, from, to, model, "mymodel"));
                }
                return list;
            };
            //Act
            Program.Main(Helper.Path, OutputFolder, Helper.FromLang, Helper.ToLang, Api, ApiKey);

            //Assert
            Helper.AssertFilesAreIdentical($"{Helper.ValidatePath}\\{Helper.ScrubsFileNF}", $"{OutputFolder}\\{Helper.ScrubsFileNF}");
            Helper.AssertFilesAreIdentical($"{Helper.ValidatePath}\\{Helper.SrtExampleFileNF}", $"{OutputFolder}\\{Helper.SrtExampleFileNF}");
            Helper.AssertMockExFilesHaveStrings($@"{OutputFolder}\{Helper.SrtExampleFileNF}", $@"{OutputFolder}\{Helper.ScrubsFileNF}");
        }
    }
}
