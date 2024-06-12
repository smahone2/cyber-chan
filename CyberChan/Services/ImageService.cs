using CyberChan.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using static CyberChan.Services.AiService;

namespace CyberChan.Services
{
    internal class ImageService(HttpClient httpClient, AiService aiService)
    {
        internal async Task<JToken> TenorGifSearch(string searchText)
        {
            var rand = new Random();

            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(new NameValueCollection
            { 
                { "key"          , ConfigurationManager.AppSettings["TenorAPI"] },
                { "q"            , searchText                                   },
                { "contentfilter", "medium"                                     },
                { "media_filter" , "basic"                                      },
                { "ar_range"     , "standard"                                   },
                { "pos"          , rand.Next(0, 190).ToString()                 }
            });

            var result = await httpClient.GetAsync(new Uri($"https://g.tenor.com/v1/search?{queryString}"));
            JObject json = JObject.Parse(await result.Content.ReadAsStringAsync());

            return json["results"][rand.Next(0, 20)];
        }

        internal async Task GenerateImageCommon(Func<string, string, string, ImageRepsonse> modelDelegate, CommandContext ctx, string query, string baseFilename)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var seed = "";

            if (query.StartsWith('<') && query.Contains('>'))
            {
                seed = query.Split("> ")[0].Replace("<", "");
                query = query.Split("> ")[1].Trim();
            }

            if (await aiService.Moderation(query) == "Pass")
            {
                DiscordMessageBuilder msg = new ();
                var imageResponse = modelDelegate(query, ctx.User.Mention, seed);
                DiscordEmbedBuilder embed = new ();

                foreach (var chunk in query.SplitBy(1024))
                {
                    embed.AddField("Original Prompt:", chunk);
                }

                if (!string.IsNullOrEmpty(imageResponse.revisedPrompt))
                {
                    foreach (var chunk in imageResponse.revisedPrompt.SplitBy(1024))
                    {
                        embed.AddField("Revised Prompt:", chunk);
                    }
                }

                if (!string.IsNullOrEmpty(imageResponse.url))
                {
                    using Stream stream = await httpClient.GetStreamAsync(imageResponse.url);
                    msg.AddFile(baseFilename, stream);

                    msg.AddEmbed(embed);
                    await ctx.RespondAsync(msg);
                }
                else
                {
                    msg.AddEmbed(embed);
                    await ctx.RespondAsync(msg);
                }
            }
            else
            {
                await ctx.RespondAsync("Query failed to pass OpenAI content moderation");
            }
        }
    }
}
