using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class DataDesc
    {
        public IReadOnlyList<MySubtitleItem> Subtitles { get; protected set; }
        public int MaxArrayItemsPerReq { get; protected set; }
        public int MaxCharactersPerRequest { get; protected set; }
        public int MaxTriesInCaseHttpFails { get; protected set; }
        public int SleepTimeIfHttpFails { get; protected set; }
        public int SleepBetweenCalls { get; protected set; }
        public Func<string, string> SendAction { get; protected set; }
        /// <summary>
        /// Dirty fix... this shouldn't be here
        /// </summary>
        public bool WaitForInput { get; protected set; }
        public long MaxCharactersToSend { get; protected set; }
        public DataDesc(IReadOnlyList<MySubtitleItem> subs, int maipr, int mcpr, int mtichf, int stihf, Func<string, string> sa, int sbc, bool wfi, long mcts)
        {
            Subtitles = subs ?? throw new ArgumentNullException(nameof(subs));
            SendAction = sa ?? throw new ArgumentNullException(nameof(sa));

            MaxArrayItemsPerReq = maipr;
            MaxCharactersPerRequest = mcpr;
            MaxTriesInCaseHttpFails = mtichf;
            SleepTimeIfHttpFails = stihf;
            SleepBetweenCalls = sbc;
            WaitForInput = wfi;
            MaxCharactersToSend = mcts;
        }
    }
}
