using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.GoogleApi
{
    public class GoogleTranslator : ITranslationService
    {
        public IList<string> Translate(string from, string to, IList<string> lines)
        {
            //TODO add delay/sleep
            return null;
        }
    }
}
