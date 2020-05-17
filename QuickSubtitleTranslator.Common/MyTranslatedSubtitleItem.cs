using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class MyTranslatedSubtitleItem
    {
        public int StartTime { get; protected set; }
        public int EndTime { get; protected set; }
        public string Line { get; protected set; }
        
        public MyTranslatedSubtitleItem(int s, int e, string l)
        {
            StartTime = s;
            EndTime = e;
            Line = l ?? throw new ArgumentNullException(nameof(l));
        }
    }
}
