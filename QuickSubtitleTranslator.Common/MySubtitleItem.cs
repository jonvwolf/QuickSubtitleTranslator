using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class MySubtitleItem
    {
        public int StartTime { get; protected set; }
        public int EndTime { get; protected set; }
        public IReadOnlyList<string> Lines { get; protected set; }

        public MySubtitleItem(int s, int e, IReadOnlyList<string> l)
        {
            StartTime = s;
            EndTime = e;
            Lines = l?.ToImmutableList() ?? throw new ArgumentNullException(nameof(l));
        }

    }
}
