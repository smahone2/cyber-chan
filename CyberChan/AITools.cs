using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using OpenAI.GPT3.Tokenizer.GPT3;
using SteamWebAPI2.Utilities;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using Slko.TraceMoeNET.Models;

namespace CyberChan
{

    class AITools
    {
        public static OpenAIService openAiService;
        public static string searchResult;

        public AITools()
        {
            openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = ConfigurationManager.AppSettings["OpenAIAPIKey"]
            });
        }

        public string GenerateImage(string query, string user)
        {
            GenerateImageTask(query, user).ConfigureAwait(false).GetAwaiter().GetResult();
            return searchResult;
        }

        async private Task GenerateImageTask(string query, string user)
        {
            var imageResult = await openAiService.Image.CreateImage(new ImageCreateRequest
            {
                Prompt = query,
                N = 1,
                Size = StaticValues.ImageStatics.Size.Size256,
                ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                User = user
            });

            if (imageResult.Successful)
            {
                searchResult = string.Join("\n", imageResult.Results.Select(r => r.Url));
            }
        }

        public string GPT3Prompt(string query, string user)
        {
            GPT3PromptTask(query, user).ConfigureAwait(false).GetAwaiter().GetResult();
            return searchResult;
        }

        async private Task GPT3PromptTask(string query, string user)
        {
            var completionResult = openAiService.Completions.CreateCompletionAsStream(new CompletionCreateRequest()
            {
                Prompt = query,
                Model = Models.TextDavinciV3,
                User = user
            });

            searchResult = "";
            await foreach (var completion in completionResult)
            {
                if (completion.Successful)
                {
                    searchResult += completion.Choices.FirstOrDefault()?.Text;
                }
                else
                {
                    if (completion.Error == null)
                    {
                        searchResult = "Unknown Error";
                    }
                    searchResult += $"{completion.Error.Code}: {completion.Error.Message}";
                }
            }
        }
    }
}
