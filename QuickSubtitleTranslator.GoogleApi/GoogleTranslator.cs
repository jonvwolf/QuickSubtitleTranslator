using Google.Cloud.Translation.V2;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace QuickSubtitleTranslator.GoogleApi
{
    public class GoogleTranslator : ITranslationService
    {
        public static Func<TranslationClient, IReadOnlyList<string>, string, string, TranslationModel, IList<TranslationResult>> SendData { get; set; }
        const int MaxArrayItemsPerReq = 128;
        const int MaxCharactersToSend = 8000;
        public static int SleepIfFails = 20000;
        public static int SleepBetweenCalls = 5500;
        public static int MaxTries = 5;

        public MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey, bool waitForInput)
        {
            if (SendData == null)
            {
                SendData = (client, subset, to, from, model) =>
                {
                    return client.TranslateText(subset, to, from, model);
                };
            }
            IList<string> SendAction(IReadOnlyList<string> subset)
            {
                using var client = TranslationClient.CreateFromApiKey(apiKey);
                var resp = SendData(client, subset, to, from, TranslationModel.NeuralMachineTranslation);
                return resp.Select(x => x.TranslatedText).ToList();
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
