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
        public static Func<LanguageTranslatorService, List<string>, string, DetailedResponse<TranslationResult>> SendData { get; set; }

        const int MaxArrayItemsPerReq = 100;
        const int MaxCharactersToSend = 5000;
        public static int SleepIfFails = 20000;
        public static int SleepBetweenCalls = 3500;
        public static int MaxTries = 5;

        public MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey, bool waitForInput)
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

            if (SendData == null)
            {
                SendData = (svc, list, modelId) =>
                {
                    return svc.Translate(list, modelId: modelId);
                };
            }

            string SendAction(string sentenceToTranslate)
            {
                IamAuthenticator authenticator = new IamAuthenticator(apikey: key);
                LanguageTranslatorService svc = null;
                try
                {
                    svc = new LanguageTranslatorService("2018-05-01", authenticator);
                    svc.SetServiceUrl(url);

                    var list = sentenceToTranslate.Split(Helper.LineBreak);

                    var response = SendData(svc, list.ToList(), $"{from}-{to}");
                    var result = response.Result.Translations.Select(x => x._Translation).ToList();

                    string translated = string.Join(Helper.LineBreak, result);

                    if (string.IsNullOrWhiteSpace(translated))
                        throw new Exception("Translated is empty");

                    return translated;
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
                sbc: SleepBetweenCalls,
                wfi: waitForInput
            ));

            return result;
        }
    }
}
