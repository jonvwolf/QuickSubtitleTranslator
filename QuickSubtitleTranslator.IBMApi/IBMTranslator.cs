using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Cloud.SDK.Core.Http;
using IBM.Watson.LanguageTranslator.v3;
using IBM.Watson.LanguageTranslator.v3.Model;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QuickSubtitleTranslator.IBMApi
{
    public class IBMTranslator : ITranslationService
    {
        public IList<string> Translate(string from, string to, IList<string> lines, string apiKey)
        {
            string key;
            string url;

            try
            {
                string[] parts = apiKey.Split("||1||");
                key = parts[0];
                url = parts[1];
            }
            catch
            {
                Console.WriteLine("ERROR: IBM api key is in a bad format. `api_key||1||url`");
                throw;
            }

            IamAuthenticator authenticator = new IamAuthenticator(apikey: key);
            LanguageTranslatorService svc = new LanguageTranslatorService("2018-05-01", authenticator);
            svc.SetServiceUrl(url);

            int maxPerRequest = 100;
            int blocks = 0;
            List<string> tempBlocks = new List<string>(maxPerRequest);
            List<string> result = new List<string>(lines.Count);
            for (int i = 0; i < lines.Count; i++)
            {
                blocks++;
                tempBlocks.Add(lines[i]);
                if (blocks >= maxPerRequest || i + 1 >= lines.Count)
                {
                    Console.WriteLine("Translating using IBM API... chunks of 100 blocks...");
                    Console.WriteLine("\tText peek: " + tempBlocks.Last());
                    //todo improve this
                    Thread.Sleep(3500);
                    var response = svc.Translate(tempBlocks, modelId: $"{from}-{to}");
                    result.AddRange(response.Result.Translations.Select(x => x._Translation));

                    Console.WriteLine("\tTranslated text peek: " + result.Last());
                    Console.WriteLine("\tTranslated character count: " + response.Result.CharacterCount);
                    blocks = 0;
                    tempBlocks.Clear();
                }
            }

            return result;
        }
    }
}
