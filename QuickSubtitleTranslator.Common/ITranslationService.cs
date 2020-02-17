using System;

namespace QuickSubtitleTranslator.Common
{
    public interface ITranslationService
    {
        string[] Translate(string from, string to, string[] lines);
    }
}
