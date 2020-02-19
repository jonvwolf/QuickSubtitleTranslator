using Amazon;
using Amazon.Translate;
using Amazon.Translate.Model;
using QuickSubtitleTranslator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace QuickSubtitleTranslator.AmazonApi
{
    public class AmazonTranslator : ITranslationService
    {
        public IList<string> Translate(string from, string to, IList<string> lines, string apiKey)
        {
            string accessKey;
            string secretKey;

            try
            {
                string[] parts = apiKey.Split("||1||");
                accessKey = parts[0];
                secretKey = parts[1];
            }
            catch
            {
                Console.WriteLine("ERROR: Amazon api key is in a bad format. `access_key||1||secret_access_key`");
                throw;
            }

            //Limits: https://docs.aws.amazon.com/translate/latest/dg/what-is-limits.html
            //max len: 3000~ (actually is 5k bytes)
            int maxTextLength = 2500;
            char delimiter = '\n';
            List<string> result = new List<string>(lines.Count);
            using(var service = new AmazonTranslateClient(accessKey, secretKey, RegionEndpoint.USEast2))
            {
                int currentLength = 0;
                StringBuilder sb = new StringBuilder(maxTextLength);
                for(int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    //Do not count with StringInfo as amazon limit is 5k bytes
                    currentLength += line.Length;
                    sb.Append(line);

                    if(currentLength >= maxTextLength || i + 1 >= lines.Count)
                    {
                        var request = new TranslateTextRequest();
                        request.Text = sb.ToString();
                        request.SourceLanguageCode = from;
                        request.TargetLanguageCode = to;
                        //todo improve this
                        Thread.Sleep(3500);
                        Console.WriteLine($"Translating using Amazon API... sending {currentLength} characters...");
                        Console.WriteLine("\tText peek: " + line);
                        var response = service.TranslateTextAsync(request).Result;

                        //save results
                        result.AddRange(response.TranslatedText.Split(delimiter));
                        Console.WriteLine("\tTranslated text peek: " + result.Last());

                        currentLength = 0;
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(delimiter);
                    }
                }
            }
            return result;
        }
    }
}
