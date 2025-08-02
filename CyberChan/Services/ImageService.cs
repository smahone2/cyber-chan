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

        internal async Task GenerateImageVariationFromMessage(TextCommandContext ctx, string instructions, string baseFilename)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var message = ctx.Message;
            if (message.MessageType == DiscordMessageType.Reply)
            {
                var reply = message.ReferencedMessage;
                string imageUrl = null;

                // Check for attachments first
                if (reply.Attachments.Count > 0)
                {
                    var attachment = reply.Attachments[0];
                    if (attachment.MediaType?.StartsWith("image/") == true)
                    {
                        imageUrl = attachment.Url;
                    }
                }
                // Check for embedded images
                else if (reply.Embeds.Count > 0 && reply.Embeds[0].Image != null)
                {
                    imageUrl = reply.Embeds[0].Image.Url.ToString();
                }

                if (imageUrl != null)
                {
                    // Parse instructions to determine operation mode
                    bool isEdit = true;
                    string processedInstructions = instructions?.Trim() ?? "";
                    
                    if (processedInstructions.StartsWith("create", StringComparison.OrdinalIgnoreCase))
                    {
                        isEdit = false;
                        processedInstructions = processedInstructions.Substring(6).Trim();
                    }
                    else if (processedInstructions.StartsWith("edit", StringComparison.OrdinalIgnoreCase))
                    {
                        isEdit = true;
                        processedInstructions = processedInstructions.Substring(4).Trim();
                    }
                    else if (string.IsNullOrEmpty(processedInstructions))
                    {
                        // Default to creating a variation if no instructions provided
                        isEdit = false;
                        processedInstructions = "Create a variation of this image";
                    }

                    DiscordMessageBuilder msg = new();
                    AiService.ImageRepsonse imageResponse;

                    if (string.IsNullOrEmpty(processedInstructions) || processedInstructions == "Create a variation of this image")
                    {
                        // Use the original variation method for simple variations
                        imageResponse = await aiService.GenerateImageVariation(imageUrl, ctx.User.Mention);
                    }
                    else
                    {
                        // Use the new GPT Vision + editing/generation method
                        imageResponse = await aiService.AnalyzeAndModifyImage(imageUrl, processedInstructions, ctx.User.Mention, isEdit);
                    }

                    DiscordEmbedBuilder embed = new();
                    embed.AddField("Source Image:", imageUrl);
                    embed.AddField("Operation:", isEdit ? "Edit" : "Create New");
                    
                    if (!string.IsNullOrEmpty(processedInstructions) && processedInstructions != "Create a variation of this image")
                    {
                        embed.AddField("Instructions:", processedInstructions);
                    }

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
                    await ctx.RespondAsync("The referenced message doesn't contain a valid image.");
                }
            }
            else
            {
                await ctx.RespondAsync("Please reply to a message containing an image. Usage: `!dallevary <edit|create> [instructions]`");
            }
        }
    }
}
