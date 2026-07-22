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
        // Recognised image-quality tokens accepted inside the `< ... >` prefix of the
        // image command. Keys are lower-case and lookup is case-insensitive (OrdinalIgnoreCase).
        private static readonly System.Collections.Generic.Dictionary<string, ImageQuality> QualityTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            ["low"] = ImageQuality.LowQuality,
            ["lowquality"] = ImageQuality.LowQuality,
            ["medium"] = ImageQuality.MediumQuality,
            ["mediumquality"] = ImageQuality.MediumQuality,
            ["high"] = ImageQuality.HighQuality,
            ["highquality"] = ImageQuality.HighQuality,
        };

        /// <summary>
        /// Splits the raw seed prefix into (remaining-seed, quality). If a quality token is
        /// present it is removed from the seed; otherwise the default quality is returned and
        /// the seed is passed through unchanged. Unrecognised tokens are left in place so
        /// they can still act as persona/style seeds (safe fallback — no validation error).
        /// </summary>
        private static (string Seed, ImageQuality Quality) ExtractQualityFromSeed(string rawSeed)
        {
            if (string.IsNullOrWhiteSpace(rawSeed))
            {
                return (rawSeed ?? string.Empty, AiService.DefaultImageQuality);
            }

            var quality = AiService.DefaultImageQuality;
            var remaining = new System.Collections.Generic.List<string>();

            foreach (var raw in rawSeed.Split(','))
            {
                var token = raw.Trim();
                if (token.Length == 0)
                {
                    continue;
                }

                if (QualityTokens.TryGetValue(token, out var mapped))
                {
                    quality = mapped;
                }
                else
                {
                    remaining.Add(token);
                }
            }

            return (string.Join(",", remaining), quality);
        }

        /// <summary>
        /// Invokes the image-generation delegate, routing to the quality-aware overload when
        /// the delegate targets <see cref="AiService.GenerateImage(string, string, string)"/>.
        /// For any other delegate the original three-argument signature is used unchanged.
        /// </summary>
        private static Task<ImageResponse> InvokeImageDelegate(
            Func<string, string, string, Task<ImageResponse>> modelDelegate,
            string query,
            string user,
            string seed,
            ImageQuality quality)
        {
            if (modelDelegate.Target is AiService ai && modelDelegate.Method.Name == nameof(AiService.GenerateImage))
            {
                return ai.GenerateImage(query, user, seed, quality);
            }

            return modelDelegate(query, user, seed);
        }

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

        internal async Task GenerateImageCommon(Func<string, string, string, Task<ImageResponse>> modelDelegate, CommandContext ctx, string query, string baseFilename)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var seed = "";
            ImageQuality quality = AiService.DefaultImageQuality;

            if (query.StartsWith('<') && query.Contains('>'))
            {
                seed = query.Split("> ")[0].Replace("<", "");
                query = query.Split("> ")[1].Trim();

                // The seed prefix may contain persona/style tokens ("simple", "detailed", ...)
                // and/or a quality token ("low", "medium", "high"). Pull out any quality token,
                // leaving the remaining tokens as the seed for BuildImagePrompt.
                (seed, quality) = ExtractQualityFromSeed(seed);
            }

            if (await aiService.Moderation(query) == "Pass")
            {
                DiscordMessageBuilder msg = new();
                var imageResponse = await InvokeImageDelegate(modelDelegate, query, ctx.User.Mention, seed, quality);
                DiscordEmbedBuilder embed = new();

                foreach (var chunk in query.SplitBy(1024))
                {
                    embed.AddField("Original Prompt:", chunk);
                }

                if (!string.IsNullOrEmpty(imageResponse.RevisedPrompt))
                {
                    foreach (var chunk in imageResponse.RevisedPrompt.SplitBy(1024))
                    {
                        embed.AddField("Revised Prompt:", chunk);
                    }
                }

                if (imageResponse.Stream != null)
                {
                    using Stream stream = imageResponse.Stream;
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

        internal async Task EditImageFromMessage(TextCommandContext ctx, string instructions, string baseFilename)
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

                    if (!string.IsNullOrEmpty(processedInstructions))
                    {
                        isEdit = true;
                        processedInstructions = processedInstructions.Trim();
                    }
                    else if (string.IsNullOrEmpty(processedInstructions))
                    {
                        // Default to creating a variation if no instructions provided
                        isEdit = false;
                        processedInstructions = "Create a variation of this image";
                    }

                    DiscordMessageBuilder msg = new();
                    AiService.ImageResponse imageResponse;

                    // Use the new GPT Vision + editing/generation method
                    imageResponse = await aiService.EditOrCreateImageFromReference(imageUrl, processedInstructions, ctx.User.Mention, isEdit);

                    DiscordEmbedBuilder embed = new();
                    embed.AddField("Source Image:", imageUrl);
                    embed.AddField("Operation:", isEdit ? "Edit" : "Create New");

                    if (!string.IsNullOrEmpty(processedInstructions) && processedInstructions != "Create a variation of this image")
                    {
                        embed.AddField("Instructions:", processedInstructions);
                    }

                    if (!string.IsNullOrEmpty(imageResponse.RevisedPrompt))
                    {
                        foreach (var chunk in imageResponse.RevisedPrompt.SplitBy(1024))
                        {
                            embed.AddField("Result:", chunk);
                        }
                    }

                    if (imageResponse.Stream != null)
                    {
                        using Stream stream = imageResponse.Stream;
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
                await ctx.RespondAsync("Please reply to a message containing an image. Usage: `!editimage [instructions]`");
            }
        }
    }
}
