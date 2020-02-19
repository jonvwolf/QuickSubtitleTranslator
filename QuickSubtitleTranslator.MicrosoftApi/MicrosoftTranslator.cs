using Newtonsoft.Json;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace QuickSubtitleTranslator.MicrosoftApi
{
    public class MicrosoftTranslator : ITranslationService
    {
        //More options: https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate
        const string Endpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={0}&from={1}";
        public IList<string> Translate(string from, string to, IList<string> lines, string apiKey)
        {
            string url = string.Format(Endpoint, to, from);

            int maxPerRequest = 100;
            int blocks = 0;
            IList<dynamic> tempBlocks = new List<dynamic>(maxPerRequest);
            List<string> result = new List<string>(lines.Count);

            using (var client = new HttpClient())
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    blocks++;
                    tempBlocks.Add(new { Text = lines[i] });
                    if (blocks >= maxPerRequest || i + 1 >= lines.Count)
                    {
                        //TODO improve this
                        Thread.Sleep(3500);
                        Console.WriteLine("Translating using Microsoft API... chunks of 100 blocks...");
                        Console.WriteLine("\tText peek: " + tempBlocks.Last().Text);

                        using (var request = new HttpRequestMessage())
                        {
                            /*
                             * service call start here
                             */
                            request.Method = HttpMethod.Post;
                            request.RequestUri = new Uri(url);
                            var requestBody = JsonConvert.SerializeObject(tempBlocks);
                            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

                            //TODO improve this
                            Thread.Sleep(3500);
                            using (var response = client.SendAsync(request).Result)
                            {
                                string resultStr = response.Content.ReadAsStringAsync().Result;
                                TranslationResult[] output = JsonConvert.DeserializeObject<TranslationResult[]>(resultStr);
                                
                                //todo improve this
                                foreach(var o in output)
                                {
                                    result.AddRange(o.Translations.Select(x => x.Text));
                                }
                            }
                            /*
                             * service call end here
                             */
                        }

                        Console.WriteLine("\tTranslated text peek: " + result.Last());
                        blocks = 0;
                        tempBlocks.Clear();
                    }
                }
            }
            
            return result;
        }
    }
}
