using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace QuickSubtitleTranslator.Common
{
    public static class Helper
    {
        static IList<string> TrySend(IReadOnlyList<string> subset,  int sleep, int tries, Func<IReadOnlyList<string>, IList<string>> SendAction)
        {
            while (tries > 0)
            {
                try
                {
                    return SendAction(subset);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception calling. {e}");
                }
                tries--;
                Thread.Sleep(sleep);
            }
            throw new Exception($"Couldn't get a response from server. Exitting...");
        }
        public static MyTranslateResult Process(DataDesc data)
        {
            List<MySubtitleItem> items = new List<MySubtitleItem>(data.Subtitles.Count);

            IList<CombinedLine> combinedLines = new List<CombinedLine>(data.Subtitles.Count);
            foreach (var item in data.Subtitles)
            {
                string combined = string.Join(" ", item.Lines);
                combinedLines.Add(new CombinedLine(l: combined, item, cc: new StringInfo(combined).LengthInTextElements));
            }

            int totalCharactersUsed = 0;
            int currentArrayCount = 0;
            int currentCharacterCount = 0;
            
            var subset = new List<CombinedLine>(data.MaxArrayItemsPerReq);
            foreach (var combinedLine in combinedLines)
            {
                bool send = false;
                if (currentArrayCount + 1 > data.MaxArrayItemsPerReq)
                    send = true;

                if (currentCharacterCount + combinedLine.CharacterCount > data.MaxCharactersPerRequest)
                    send = true;

                if (send)
                {
                    //COPIED CODE
                    var stringListToSend = subset.Select(x => x.Line).ToImmutableList();

                    Console.WriteLine($"Sending {currentCharacterCount} characters. Blocks {currentArrayCount}");
                    Console.WriteLine($"Peek: {stringListToSend.Last()}");

                    var translatedLines = TrySend(stringListToSend, sleep: data.SleepTimeIfHttpFails, tries: data.MaxTriesInCaseHttpFails, data.SendAction);
                    items.AddRange(Helper.Convert(translatedLines.ToImmutableList(), subset.ToImmutableList()));

                    Console.WriteLine($"Peek: {translatedLines.Last()}");

                    subset.Clear();
                    currentArrayCount = 0;
                    currentCharacterCount = 0;
                }

                currentArrayCount++;
                
                totalCharactersUsed += combinedLine.CharacterCount;
                currentCharacterCount += combinedLine.CharacterCount;
                subset.Add(combinedLine);
            }

            if (subset.Count > 0)
            {
                //COPIED CODE
                var stringListToSend = subset.Select(x => x.Line).ToImmutableList();

                Console.WriteLine($"Sending {currentCharacterCount} characters. Blocks {currentArrayCount}");
                Console.WriteLine($"Peek: {stringListToSend.Last()}");

                var translatedLines = TrySend(stringListToSend, sleep: data.SleepTimeIfHttpFails, tries: data.MaxTriesInCaseHttpFails, data.SendAction);
                items.AddRange(Helper.Convert(translatedLines.ToImmutableList(), subset.ToImmutableList()));

                Console.WriteLine($"Peek: {translatedLines.Last()}");

                subset.Clear();
            }

            return new MyTranslateResult(items, totalCharactersUsed);
        }
        public static IList<MySubtitleItem> Convert(IReadOnlyList<string> translatedLines, IReadOnlyList<CombinedLine> subset)
        {
            if (translatedLines.Count != subset.Count)
                throw new Exception($"{nameof(translatedLines)} count does not match with {nameof(subset)}");

            var list = new List<MySubtitleItem>(subset.Count);

            for (int index = 0; index < translatedLines.Count; index++)
            {
                list.Add(
                    new MySubtitleItem(
                        s: subset[index].BelongsTo.StartTime,
                        e: subset[index].BelongsTo.EndTime,
                        l: SplitWords(translatedLines[index], Constants.DoNotBreakIfOnlyLine, Constants.MaxWordsPerLine).ToImmutableList())
                    );
            }

            return list;
        }

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
