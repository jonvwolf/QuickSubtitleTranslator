using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.MicrosoftApi
{
    /// <summary>
    /// From: https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translate?pivots=programming-language-csharp
    /// </summary>
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translate?pivots=programming-language-csharp
    /// </summary>
    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translate?pivots=programming-language-csharp
    /// </summary>
    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translate?pivots=programming-language-csharp
    /// </summary>
    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translate?pivots=programming-language-csharp
    /// </summary>
    public class Alignment
    {
        public string Proj { get; set; }
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translate?pivots=programming-language-csharp
    /// </summary>
    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }
}
