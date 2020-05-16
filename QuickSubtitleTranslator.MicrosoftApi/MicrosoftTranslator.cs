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
        const int MaxArrayItemsPerReq = 100;
        const int MaxCharactersToSend = 5000;
        const int SleepIfFails = 20000;
        const int SleepBetweenCalls = 3500;
        const int MaxTries = 5;
        
        //More options: https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate
        const string Endpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={0}&from={1}";
        public MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey)
        {
            string url = string.Format(Endpoint, to, from);

            IList<string> SendAction(IReadOnlyList<string> subset)
            {
                using var client = new HttpClient();
                using var request = new HttpRequestMessage();

                var list = new List<dynamic>(subset.Count);
                foreach (var item in subset)
                {
                    list.Add(new { Text = item });
                }

                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(url);
                var requestBody = JsonConvert.SerializeObject(list);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

                var result = new List<string>(list.Count);
                using (var response = client.SendAsync(request).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string resultStr = response.Content.ReadAsStringAsync().Result;
                    TranslationResult[] output = JsonConvert.DeserializeObject<TranslationResult[]>(resultStr);

                    foreach (var o in output)
                    {
                        result.AddRange(o.Translations.Select(x => x.Text));
                    }
                }

                return result;
            }

            var result = Helper.Process(new DataDesc(
                subs: subtitles,
                maipr: MaxArrayItemsPerReq,
                mcpr: MaxCharactersToSend,
                mtichf: MaxTries,
                stihf: SleepIfFails,
                sa: SendAction,
                sbc: SleepBetweenCalls
            ));

            return result;
        }

    }
}
