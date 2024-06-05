using CyberChan.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static CyberChan.Services.AiService;

namespace CyberChan.Services
{
    internal class ImageService(HttpClient httpClient, AiService aiService)
    {
        internal async Task GenerateImageCommon(Func<string, string, string, ImageRepsonse> modelDelegate, CommandContext ctx, string query, string baseFilename)
        {
            //await ctx.TriggerTypingAsync();

            var seed = "";

            if (query.StartsWith('<') && query.Contains('>'))
            {
                seed = query.Split("> ")[0].Replace("<", "");
                query = query.Split("> ")[1].Trim();
            }

            if (await aiService.Moderation(query) == "Pass")
            {
                DiscordMessageBuilder msg = new DiscordMessageBuilder();
                var imageResponse = modelDelegate(query, ctx.User.Mention, seed);
                var embed = new DiscordEmbedBuilder();

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
