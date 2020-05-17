using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class LineFormatter
    {
        public static IList<string> SplitWords(string sentence, int doNotBreakIfOnlyOneLine, int maxWordsPerLine)
        {
            //Break up lines by "- "
            //Divide word by 2 and try to find a comma to break the line
            //Break up by <i></i>

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
