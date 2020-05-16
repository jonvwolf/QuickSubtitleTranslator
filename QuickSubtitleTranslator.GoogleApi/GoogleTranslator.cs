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
        const int MaxArrayItemsPerReq = 128;
        const int MaxCharactersToSend = 8000;
        const int SleepIfFails = 20000;
        const int SleepBetweenCalls = 5500;
        const int MaxTries = 5;

        public MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey)
        {
            IList<string> SendAction(IReadOnlyList<string> subset)
            {
                using var client = TranslationClient.CreateFromApiKey(apiKey);
                var resp = client.TranslateText(subset, to, from, TranslationModel.NeuralMachineTranslation);
                return resp.Select(x => x.TranslatedText).ToList();
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
