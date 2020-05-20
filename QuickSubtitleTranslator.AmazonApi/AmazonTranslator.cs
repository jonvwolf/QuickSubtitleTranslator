using Amazon;
using Amazon.Translate;
using Amazon.Translate.Model;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace QuickSubtitleTranslator.AmazonApi
{
    public class AmazonTranslator : ITranslationService
    {
        public static Func<AmazonTranslateClient, TranslateTextRequest, TranslateTextResponse> SendData { get; set; }

        const int MaxArrayItemsPerReq = 75;
        const int MaxCharactersToSend = 1700;
        public static int SleepIfFails = 40000;
        public static int SleepBetweenCalls = 12000;
        public static int MaxTries = 5;

        public MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey, bool waitForInput, long maxCharacterToSend)
        {
            string accessKey;
            string secretKey;

            try
            {
                string[] parts = apiKey.Split("||1||");
                accessKey = parts[0];
                secretKey = parts[1];
            }
            catch
            {
                Console.WriteLine("ERROR: Amazon api key is in a bad format. `access_key||1||secret_access_key`");
                throw;
            }

            string SendAction(string sentenceToTranslate)
            {
                if (SendData == null)
                {
                    SendData = (svc, request) =>
                    {
                        return svc.TranslateTextAsync(request).Result;
                    };
                }

                //TODO : not sure which one to believe...
                // Limits (says utf8 5k bytes): https://docs.aws.amazon.com/translate/latest/dg/what-is-limits.html
                //quota in aws says 10k bytes per 10 seconds: https://us-east-2.console.aws.amazon.com/servicequotas/home?region=us-east-2#!/services/translate/quotas

                //TODO: with the default quota values.. this is the most reliable (1920 bytes per 10.5 seconds)
                // -> int maxBytes = 1920;
                // -> Characters to add because of JSON request = {"SourceLanguageCode": "en","TargetLanguageCode": "es","TerminologyNames": null,"Text": ""}
                // -> int addedBytes = 113;

                //Answer: https://stackoverflow.com/questions/33889673/translate-api-user-rate-limit-exceeded-403-without-reason
                using var service = new AmazonTranslateClient(accessKey, secretKey, RegionEndpoint.USEast2);

                //Amazon deletes anything between dashes -> - string -
                char maybeItIsABug = '~';
                char charToReplace = '-';
                sentenceToTranslate = sentenceToTranslate.Replace(charToReplace, maybeItIsABug);

                var request = new TranslateTextRequest
                {
                    Text = sentenceToTranslate,
                    SourceLanguageCode = from,
                    TargetLanguageCode = to
                };

                var response = SendData(service, request);
                var translatedText = response.TranslatedText.Replace(maybeItIsABug, charToReplace);

                if (string.IsNullOrWhiteSpace(translatedText))
                    throw new Exception("Translated text from amazon was empty");

                return translatedText;
            }

            var result = Helper.Process(new DataDesc(
                subs: subtitles,
                maipr: MaxArrayItemsPerReq,
                mcpr: MaxCharactersToSend,
                mtichf: MaxTries,
                stihf: SleepIfFails,
                sa: SendAction,
                sbc: SleepBetweenCalls,
                wfi: waitForInput,
                mcts: maxCharacterToSend
            ));

            return result;
        }
    }
}
