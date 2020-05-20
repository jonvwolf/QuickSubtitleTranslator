using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class MyTranslateResult
    {
        public IReadOnlyList<MySubtitleItem> TranslatedItems { get; protected set; }
        public int TranslatedCharacters { get; protected set; }
        public bool SkippedBecauseOfCharacterLimit { get; protected set; } = false;
        public MyTranslateResult(IReadOnlyList<MySubtitleItem> items, int count)
        {
            TranslatedItems = items ?? throw new ArgumentNullException(nameof(items));
            TranslatedCharacters = count;
        }

        public MyTranslateResult()
        {
            SkippedBecauseOfCharacterLimit = true;
        }
    }
}
