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
        public const string LineBreak = "\n";
        
        static string TrySend(string sentenceToTranslate, int sleep, int tries, Func<string, string> SendAction, bool waitForInput)
        {
            var list = new List<Exception>();

            int currentTries = tries;
        
            while (currentTries > 0)
            {
                currentTries--;

                try
                {
                    return SendAction(sentenceToTranslate);
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
                    if (AskToContinue(waitForInput))
                        currentTries = tries;
                    else
                        break;
                }

                Console.WriteLine($"Sleeping for {sleep}. Current tries: {currentTries}");
                Thread.Sleep(sleep);
            }

            throw new Exception($"Couldn't get a response from server. See inner exception. Exitting...", new AggregateException(list));
        }
        static bool AskToContinue(bool waitForInput)
        {
            if (!waitForInput)
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
            var translatedItems = new List<MySubtitleItem>(data.Subtitles.Count);
            IList<LineToTranslate> linesToTranslate = new List<LineToTranslate>((int)(data.Subtitles.Count * 1.5));
            
            foreach (var item in data.Subtitles)
            {
                //translatedItems.Count - 1 is the index of to which it belongs to (line <=> MySubtitleItem)
                //This is needed to fill the lines of each mysubtitleitem
                translatedItems.Add(new MySubtitleItem(s: item.StartTime, e: item.EndTime));
                foreach (var line in item.Lines)
                {
                    linesToTranslate.Add(new LineToTranslate(l: line + LineBreak, item, index: translatedItems.Count - 1));
                }
            }

            int totalCharactersToSend = linesToTranslate.Sum(x => x.CharacterCount);

            if (data.MaxCharactersToSend > 0 && totalCharactersToSend > data.MaxCharactersToSend)
                return new MyTranslateResult();

            int totalCharactersUsed = 0;
            int currentArrayCount = 0;
            int currentCharacterCount = 0;
            bool afterFirstCall = false;
            int totalProcessedCount = 0;
            var subset = new List<LineToTranslate>(data.MaxArrayItemsPerReq);

            foreach (var line in linesToTranslate)
            {
                //Add
                currentArrayCount++;
                totalProcessedCount++;
                totalCharactersUsed += line.CharacterCount;
                currentCharacterCount += line.CharacterCount;
                subset.Add(line);

                //Check
                bool send = false;
                if (currentArrayCount >= data.MaxArrayItemsPerReq)
                    send = true;

                if (currentCharacterCount >= data.MaxCharactersPerRequest)
                    send = true;

                if (totalProcessedCount >= linesToTranslate.Count)
                    send = true;

                //Send
                if (send)
                {
                    if (afterFirstCall)
                        Thread.Sleep(data.SleepBetweenCalls);

                    afterFirstCall = true;
                    //Trim() to remove the last \n so it doesn't get splitted into an emptry string
                    var sentenceToSend = string.Join(string.Empty, subset.Select(x => x.Line)).Trim();
                    
                    var translatedSentence = TrySend(sentenceToSend, sleep: data.SleepTimeIfHttpFails, tries: data.MaxTriesInCaseHttpFails, data.SendAction, data.WaitForInput);

                    FillResult(translatedSentence, subset, translatedItems);
                    subset.Clear();

                    Console.WriteLine($"Processed {totalCharactersUsed} out of {totalCharactersToSend} characters");
                    Console.WriteLine($"Sent {currentCharacterCount} characters ({Encoding.UTF8.GetByteCount(sentenceToSend + '\n')} bytes in UTF8). Sent Lines {currentArrayCount}");
                    if (data.Peek)
                        Console.WriteLine($"Peek: {translatedItems.Last(x => x.Lines.Count > 0).Lines.Last()}");

                    currentArrayCount = 0;
                    currentCharacterCount = 0;
                }
            }

            if (subset.Count > 0 || totalProcessedCount != linesToTranslate.Count)
                throw new Exception($"{nameof(subset)} has unprocessed items. Total processed: {totalProcessedCount} : Total count: {linesToTranslate.Count}");

            return new MyTranslateResult(translatedItems, totalCharactersUsed);
        }
        public static void FillResult(string translatedSentence, IReadOnlyList<LineToTranslate> subset, IReadOnlyList<MySubtitleItem> listToFill)
        {
            if (string.IsNullOrWhiteSpace(translatedSentence))
                throw new ArgumentNullException(nameof(translatedSentence));

            var lines = translatedSentence.Split(LineBreak).ToList();

            if (lines.Count != subset.Count)
                throw new Exception($"Translated lines and what was sent does not match");

            for (int index = 0; index < subset.Count; index++)
            {
                var subsetItem = subset[index];
                var translatedItem = lines[index];
                var itemToFill = listToFill[subsetItem.BelongsToIndex];

                itemToFill.AddLine(translatedItem);

                if (subsetItem.BelongsTo.StartTime != itemToFill.StartTime || subsetItem.BelongsTo.EndTime != itemToFill.EndTime)
                    throw new Exception($"Index is out of sync, between subset and listToFill");
            }
        }

    }
}
