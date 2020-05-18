using System;
using System.Collections.Generic;

namespace QuickSubtitleTranslator.Common
{
    public interface ITranslationService
    {
        MyTranslateResult Translate(string from, string to, IReadOnlyList<MySubtitleItem> subtitles, string apiKey, bool waitForInput);
    }
}
