using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class LineToTranslate
    {
        public string Line { get; protected set; }
        public MySubtitleItem BelongsTo { get; protected set; }
        public int BelongsToIndex { get; protected set; }
        public int CharacterCount { get; protected set; }

        public LineToTranslate(string l, MySubtitleItem bt, int index)
        {
            Line = l ?? throw new ArgumentNullException(nameof(l));
            BelongsTo = bt ?? throw new ArgumentNullException(nameof(bt));

            CharacterCount = new StringInfo(Line).LengthInTextElements;
            BelongsToIndex = index;
        }
    }
}
