using Google.Cloud.Translation.V2;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace QuickSubtitleTranslator.GoogleApi
{
    public class GoogleTranslator : ITranslationService
    {
        public IList<string> Translate(string from, string to, IList<string> lines, string apiKey)
        {
            //TODO: make this better. creating a client for every file...
            using (TranslationClient client = TranslationClient.CreateFromApiKey(apiKey))
            {
                int maxPerRequest = 128;
                int blocks = 0;
                IList<string> tempBlocks = new List<string>(maxPerRequest);
                List<string> result = new List<string>(lines.Count);
                for (int i = 0; i < lines.Count; i++)
                {
                    blocks++;
                    tempBlocks.Add(lines[i]);
                    if (blocks >= maxPerRequest || i + 1 >= lines.Count)
                    {
                        //TODO improve this
                        Thread.Sleep(3500);
                        Console.WriteLine("Translating using Google API... chunks of 128 blocks...");
                        Console.WriteLine("Text peek: " + tempBlocks.Last());
                        var resp = client.TranslateText(tempBlocks, to, from, TranslationModel.NeuralMachineTranslation);
                        result.AddRange(resp.Select(x => x.TranslatedText).ToList());
                        Console.WriteLine("Translated text peek: " + result.Last());
                        blocks = 0;
                        tempBlocks.Clear();
                    }
                }

                return result;
            }
        }
    }
}
