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
        public Func<IReadOnlyList<string>, IList<string>> SendAction { get; protected set; }
        public DataDesc(IReadOnlyList<MySubtitleItem> subs,int maipr, int mcpr, int mtichf, int stihf, Func<IReadOnlyList<string>, IList<string>> sa)
        {
            Subtitles = subs ?? throw new ArgumentNullException(nameof(subs));
            SendAction = sa ?? throw new ArgumentNullException(nameof(sa));

            MaxArrayItemsPerReq = maipr;
            MaxCharactersPerRequest = mcpr;
            MaxTriesInCaseHttpFails = mtichf;
            SleepTimeIfHttpFails = stihf;
        }
    }
}
