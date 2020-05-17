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
        public static bool WaitForInput = false;
        static IList<string> TrySend(IReadOnlyList<string> subset, int sleep, int tries, Func<IReadOnlyList<string>, IList<string>> SendAction)
        {
            var list = new List<Exception>();

            int currentTries = tries;
        
            while (currentTries > 0)
            {
                currentTries--;

                try
                {
                    return SendAction(subset);
                }
                catch (MyException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    list.Add(e);
                    Console.WriteLine($"Exception calling. {e}");
                }
                
                if (currentTries <= 0)
                {
                    if (AskToContinue())
                        currentTries = tries;
                    else
                        break;
                }

                Console.WriteLine($"Sleeping for {sleep}. Current tries: {currentTries}");
                Thread.Sleep(sleep);
            }

            throw new Exception($"Couldn't get a response from server. See inner exception. Exitting...", new AggregateException(list));
        }
        static bool AskToContinue()
        {
            if (!WaitForInput)
                return false;

            while (true)
            {
                Console.WriteLine($"Time: {DateTime.Now}");
                Console.WriteLine("Try again or quit? [try] [quit]");
                var ans = Console.ReadLine().Trim();
                if (ans.ToUpperInvariant() == "try")
                {
                    return true;
                }
                else if (ans.ToUpperInvariant() == "quit")
                {
                    Console.WriteLine("Quitting...");
                    return false;
                }
                else
                {
                    Console.WriteLine("Invalid answer.");
                }
            }
        }
        public static MyTranslateResult Process(DataDesc data)
        {
            var items = new List<MyTranslatedSubtitleItem>(data.Subtitles.Count);

            IList<CombinedLine> combinedLines = new List<CombinedLine>(data.Subtitles.Count);
            foreach (var item in data.Subtitles)
            {
                string combined = string.Join(" ", item.Lines);
                combinedLines.Add(new CombinedLine(l: combined, item, cc: new StringInfo(combined).LengthInTextElements));
            }

            int totalCharactersUsed = 0;
            int currentArrayCount = 0;
            int currentCharacterCount = 0;
            bool afterFirstCall = false;
            int totalProcessedCount = 0;
            var subset = new List<CombinedLine>(data.MaxArrayItemsPerReq);

            foreach (var combinedLine in combinedLines)
            {
                //Add
                currentArrayCount++;
                totalProcessedCount++;
                totalCharactersUsed += combinedLine.CharacterCount;
                currentCharacterCount += combinedLine.CharacterCount;
                subset.Add(combinedLine);

                //Check
                bool send = false;
                if (currentArrayCount >= data.MaxArrayItemsPerReq)
                    send = true;

                if (currentCharacterCount >= data.MaxCharactersPerRequest)
                    send = true;

                if (totalProcessedCount >= combinedLines.Count)
                    send = true;

                //Send
                if (send)
                {
                    if (afterFirstCall)
                        Thread.Sleep(data.SleepBetweenCalls);

                    afterFirstCall = true;
                    var stringListToSend = subset.Select(x => x.Line).ToImmutableList();
                    var translatedLines = TrySend(stringListToSend, sleep: data.SleepTimeIfHttpFails, tries: data.MaxTriesInCaseHttpFails, data.SendAction);
                    items.AddRange(Helper.Convert(translatedLines.ToImmutableList(), subset.ToImmutableList()));
                    subset.Clear();

                    Console.WriteLine($"Sending {currentCharacterCount} characters. Blocks {currentArrayCount}");
                    Console.WriteLine($"Peek: {stringListToSend.Last()}");
                    Console.WriteLine($"Peek: {translatedLines.Last()}");

                    currentArrayCount = 0;
                    currentCharacterCount = 0;
                }
            }

            if (subset.Count > 0 || totalProcessedCount != combinedLines.Count)
                throw new Exception($"{nameof(subset)} has unprocessed items. Total processed: {totalProcessedCount} : Total count: {combinedLines.Count}");

            return new MyTranslateResult(items, totalCharactersUsed);
        }
        public static IList<MyTranslatedSubtitleItem> Convert(IReadOnlyList<string> translatedLines, IReadOnlyList<CombinedLine> subset)
        {
            if (translatedLines.Count != subset.Count)
                throw new Exception($"{nameof(translatedLines)} count does not match with {nameof(subset)}");

            var list = new List<MyTranslatedSubtitleItem>(subset.Count);

            for (int index = 0; index < translatedLines.Count; index++)
            {
                list.Add(
                    new MyTranslatedSubtitleItem(
                        s: subset[index].BelongsTo.StartTime,
                        e: subset[index].BelongsTo.EndTime,
                        l: translatedLines[index]
                    ));
            }

            return list;
        }

    }
}
