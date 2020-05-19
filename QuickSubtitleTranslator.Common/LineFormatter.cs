using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QuickSubtitleTranslator.Common
{
    public class LineFormatter
    {
        public static IList<string> SplitWords(string sentence, int doNotBreakIfOnlyOneLine, int maxWordsPerLine)
        {
            string[] split = sentence.Split(" ");
            if (split.Length <= doNotBreakIfOnlyOneLine)
                return new List<string>() { sentence };

            var list = new List<string>();
            int wordCount = 0;
            int remaining = split.Length;
            StringBuilder sb = new StringBuilder(sentence.Length);
            foreach (var word in split)
            {
                bool add = false;
                bool addLastOne = false;
                if (wordCount + 1 > maxWordsPerLine)
                    add = true;
                if (remaining - 1 <= 0)
                {
                    add = true;
                    addLastOne = true;
                }

                if (add)
                {
                    if (addLastOne)
                    {
                        remaining--;
                        sb.Append(word.Trim()).Append(" ");
                    }

                    list.Add(sb.ToString().Trim());
                    wordCount = 0;
                    sb.Clear();
                }

                if (!addLastOne)
                {
                    wordCount++;
                    remaining--;
                    sb.Append(word.Trim()).Append(" ");
                }
            }
            if (sb.ToString().Length > 0 || remaining > 0)
                throw new Exception($"Bug. {nameof(sb)} still has characters...");

            return list;
        }
    }
}
