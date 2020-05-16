using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class CombinedLine
    {
        public string Line { get; protected set; }
        public MySubtitleItem BelongsTo { get; protected set; }
        public int CharacterCount { get; protected set; }

        public CombinedLine(string l, MySubtitleItem bt, int cc)
        {
            Line = l ?? throw new ArgumentNullException(nameof(l));
            BelongsTo = bt ?? throw new ArgumentNullException(nameof(bt));
            CharacterCount = cc;

            if (CharacterCount <= 0)
                throw new ArgumentException($"Invalid value of {nameof(cc)}");
        }
    }
}
