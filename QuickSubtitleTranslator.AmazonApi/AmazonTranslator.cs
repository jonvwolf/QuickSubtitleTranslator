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

            //TODO : not sure which one to believe...
            // Limits (says utf8 5k bytes): https://docs.aws.amazon.com/translate/latest/dg/what-is-limits.html
            //quota in aws says 10k bytes per 10 seconds: https://us-east-2.console.aws.amazon.com/servicequotas/home?region=us-east-2#!/services/translate/quotas

            //TODO: with the default quota values.. this is the most reliable (1920 bytes per 10.5 seconds)
            //Answer: https://stackoverflow.com/questions/33889673/translate-api-user-rate-limit-exceeded-403-without-reason
            int maxBytes = 1920;
            //Characters to add because of JSON request = {"SourceLanguageCode": "en","TargetLanguageCode": "es","TerminologyNames": null,"Text": ""}
            int addedBytes = 113;
            char delimiter = '\n';
            List<string> result = new List<string>(lines.Count);
            using(var service = new AmazonTranslateClient(accessKey, secretKey, RegionEndpoint.USEast2))
            {
                int currentBytes = 0;
                StringBuilder sb = new StringBuilder(maxBytes);
                for(int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    //TODO: fix this...
                    //TODO: add delimiter char
                    currentBytes += Encoding.UTF8.GetByteCount(line) + 1; //1 = delimiter char
                    sb.Append(line);

                    if(currentBytes + addedBytes >= maxBytes || i + 1 >= lines.Count)
                    {
                        var request = new TranslateTextRequest();
                        request.Text = sb.ToString();
                        request.SourceLanguageCode = from;
                        request.TargetLanguageCode = to;
                        
                        //todo improve this
                        Thread.Sleep(10500);
                        Console.WriteLine($"Translating using Amazon API... sending {Encoding.UTF8.GetByteCount(request.Text) + addedBytes} bytes (UTF8)... Length: {request.Text.Length + addedBytes}...");
                        Console.WriteLine("\tText peek: " + line);
                        var response = service.TranslateTextAsync(request).Result;
                        
                        //save results
                        result.AddRange(response.TranslatedText.Split(delimiter));
                        Console.WriteLine("\tTranslated text peek: " + result.Last());

                        currentBytes = 0;
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
