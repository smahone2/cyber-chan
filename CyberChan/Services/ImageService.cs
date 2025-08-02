using CyberChan.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
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

        internal async Task GenerateImageCommon(Func<string, string, string, Task<ImageRepsonse>> modelDelegate, CommandContext ctx, string query, string baseFilename)
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
                DiscordMessageBuilder msg = new();
                var imageResponse = await modelDelegate(query, ctx.User.Mention, seed);
                DiscordEmbedBuilder embed = new();

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

        internal async Task GenerateImageVariationFromMessage(TextCommandContext ctx, string baseFilename)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var message = ctx.Message;
            if (message.MessageType == DiscordMessageType.Reply)
            {
                var reply = message.ReferencedMessage;
                if (reply.Attachments.Count > 0)
                {
                    var attachment = reply.Attachments[0];
                    if (attachment.MediaType?.StartsWith("image/") == true)
                    {
                        DiscordMessageBuilder msg = new();
                        var imageResponse = await aiService.GenerateImageVariation(attachment.Url, ctx.User.Mention);
                        DiscordEmbedBuilder embed = new();

                        embed.AddField("Source Image:", attachment.Url);

                        if (!string.IsNullOrEmpty(imageResponse.revisedPrompt))
                        {
                            foreach (var chunk in imageResponse.revisedPrompt.SplitBy(1024))
                            {
                                embed.AddField("Result:", chunk);
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
                        await ctx.RespondAsync("The referenced message doesn't contain a valid image attachment.");
                    }
                }
                else if (reply.Embeds.Count > 0 && reply.Embeds[0].Image != null)
                {
                    var imageUrl = reply.Embeds[0].Image.Url.ToString();
                    
                    DiscordMessageBuilder msg = new();
                    var imageResponse = await aiService.GenerateImageVariation(imageUrl, ctx.User.Mention);
                    DiscordEmbedBuilder embed = new();

                    embed.AddField("Source Image:", imageUrl);

                    if (!string.IsNullOrEmpty(imageResponse.revisedPrompt))
                    {
                        foreach (var chunk in imageResponse.revisedPrompt.SplitBy(1024))
                        {
                            embed.AddField("Result:", chunk);
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
                    await ctx.RespondAsync("The referenced message doesn't contain an image.");
                }
            }
            else
            {
                await ctx.RespondAsync("Please reply to a message containing an image to generate variations.");
            }
        }
    }
}
