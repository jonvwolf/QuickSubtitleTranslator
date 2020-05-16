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
        const int MaxArrayItemsPerReq = 100;
        const int MaxCharactersToSend = 5000;
        const int SleepIfFails = 20000;
        const int SleepBetweenCalls = 3500;
        const int MaxTries = 5;

        public MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey)
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

            IList<string> SendAction(IReadOnlyList<string> subset)
            {
                IamAuthenticator authenticator = new IamAuthenticator(apikey: key);
                LanguageTranslatorService svc = null;
                try
                {
                    svc = new LanguageTranslatorService("2018-05-01", authenticator);
                    svc.SetServiceUrl(url);

                    var response = svc.Translate(subset.ToList(), modelId: $"{from}-{to}");
                    var result = response.Result.Translations.Select(x => x._Translation).ToList();
                    return result;
                }
                finally
                {
                    svc?.Client?.Dispose();
                }
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
