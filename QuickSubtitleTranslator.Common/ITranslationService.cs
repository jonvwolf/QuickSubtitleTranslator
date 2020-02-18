using System;
using System.Collections.Generic;

namespace QuickSubtitleTranslator.Common
{
    public interface ITranslationService
    {
        IList<string> Translate(string from, string to, IList<string> lines, string apiKey);
    }
}
