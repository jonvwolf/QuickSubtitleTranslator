using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class MyTranslateResult
    {
        public IList<MyTranslatedSubtitleItem> TranslatedItems { get; protected set; }
        public int TranslatedCharacters { get; protected set; }
        public MyTranslateResult(IList<MyTranslatedSubtitleItem> items, int count)
        {
            TranslatedItems = items ?? throw new ArgumentNullException(nameof(items));
            TranslatedCharacters = count;
        }
    }
}
