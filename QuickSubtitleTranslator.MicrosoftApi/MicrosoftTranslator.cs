using Newtonsoft.Json;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace QuickSubtitleTranslator.MicrosoftApi
{
    public class MicrosoftTranslator : ITranslationService
    {
        public static Func<HttpClient, HttpRequestMessage, HttpResponseMessage> SendData { get; set; }

        const int MaxArrayItemsPerReq = 100;
        const int MaxCharactersToSend = 5000;
        public static int SleepIfFails = 20000;
        public static int SleepBetweenCalls = 3500;
        public static int MaxTries = 5;
        
        //More options: https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate
        const string Endpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={0}&from={1}";
        public MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey, bool waitForInput)
        {
            string url = string.Format(Endpoint, to, from);

            if (SendData == null)
            {
                SendData = (client, request) =>
                {
                    return client.SendAsync(request).Result;
                };
            }

            string SendAction(string sentenceToTranslate)
            {
                using var client = new HttpClient();
                using var request = new HttpRequestMessage();

                var list = new List<dynamic>
                {
                    new { Text = sentenceToTranslate }
                };

                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(url);
                var requestBody = JsonConvert.SerializeObject(list);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

                var result = new List<string>(list.Count);
                using (var response = SendData(client, request))
                {
                    response.EnsureSuccessStatusCode();
                    string resultStr = response.Content.ReadAsStringAsync().Result;
                    TranslationResult[] output = JsonConvert.DeserializeObject<TranslationResult[]>(resultStr);

                    foreach (var o in output)
                    {
                        result.AddRange(o.Translations.Select(x => x.Text));
                    }
                }

                if (result.Count != 1)
                    throw new Exception($"Got result but returned more than 1 translated item");

                return result[0];
            }

            var result = Helper.Process(new DataDesc(
                subs: subtitles,
                maipr: MaxArrayItemsPerReq,
                mcpr: MaxCharactersToSend,
                mtichf: MaxTries,
                stihf: SleepIfFails,
                sa: SendAction,
                sbc: SleepBetweenCalls,
                wfi: waitForInput
            ));

            return result;
        }

    }
}
