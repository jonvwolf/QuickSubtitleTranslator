using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.MicrosoftApi
{
    public class MicrosoftTranslator : ITranslationService
    {
        public IList<string> Translate(string from, string to, IList<string> lines, string apiKey)
        {
            //TODO delay
            //TODO see limits per api request
            return null;
        }
    }
}
