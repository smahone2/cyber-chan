using CyberChan.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyberChan.Services
{
    internal class CommandsService(Giphy giphy, TraceDotMoeService traceDotMoeService, KitsuService kitsuService, AiService aiService, ImageService imageService) : Commands
    {
        public override async ValueTask Help(TextCommandContext ctx, string command = "", [RemainingText] string extraText = "")
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                DiscordEmbedBuilder embed = new()
                {
                    Color = DiscordColor.Azure,
                    Title = "Help",
                    Description = "Listing all top-level commands and groups. Specify a command to see more information.\n\n"
                };

                var methodList = GetType().GetMethods().ToList();
                var commandText = methodList
                    .Where(method => method.GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault() != null)
                    .Select(method =>
                    {
                        var textAliasAttribute = method.GetCustomAttributes(typeof(TextAliasAttribute), true).FirstOrDefault() as TextAliasAttribute;

                        return ("`" + textAliasAttribute?.Aliases.FirstOrDefault() ?? method.Name) + "`";
                    });

                embed.AddField("Commands", string.Join(", ", commandText));

                await ctx.RespondAsync(embed);
            }
            else
            {
                //DiscordEmbedBuilder embed = new()
                //{
                //    Color = DiscordColor.Azure,
                //    Title = "Help"
                //};

                //var methodList = GetType().GetMethods().ToList();
                //var commandText = methodList
                //    .Where(method => method.GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault() != null)
                //    .Select(method =>
                //    {
                //        var textAliasAttribute = method.GetCustomAttributes(typeof(TextAliasAttribute), true).FirstOrDefault() as TextAliasAttribute;

                //        return ("`" + textAliasAttribute?.Aliases.FirstOrDefault() ?? method.Name) + "`";
                //    });

                await ctx.RespondAsync("Not implemented yet");
            }
        }

        public override async ValueTask Hi(TextCommandContext ctx, string extraText = "")
        {
            Log.Information("Hi command initiated");

            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            RandomParameter giphyParameters = new()
            {
                Tag = "Hi"
            };

            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = (await giphy.RandomGif(giphyParameters)).Data.Images.Downsized.Url
            };

            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!", embed);

            Log.Information("Hi command finished");
        }

        public override async ValueTask Bye(TextCommandContext ctx, string extraText = "")
        {
            Log.Information("Bye command initiated");

            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            RandomParameter giphyParameters = new()
            {
                Tag = "Bye"
            };
            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = (await giphy.RandomGif(giphyParameters)).Data.Images.Downsized.Url
            };

            await ctx.RespondAsync($"👋 Bye, {ctx.User.Mention}!", embed);

            Log.Information("Bye command finished");
        }

        public override async ValueTask Waifu(TextCommandContext ctx, string extraText = "")
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var result = await imageService.TenorGifSearch("Anime Girl");

            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = result["media"][0]["gif"]["url"].ToString()
            };

            await ctx.RespondAsync($"{ctx.User.Mention}, here is your waifu!", embed);
        }

        public override async ValueTask Gif(TextCommandContext ctx, string searchText = null)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var result = await imageService.TenorGifSearch(string.IsNullOrWhiteSpace(searchText) ? "random" : searchText);

            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = result["media"][0]["gif"]["url"].ToString()
            };

            await ctx.RespondAsync($"{ctx.User.Mention}, here is your gif!", embed);
        }

        public override async ValueTask LookupAnime(TextCommandContext ctx, string extraText = "")
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var message = ctx.Message;
            if (message.MessageType == DiscordMessageType.Reply)
            {
                var reply = message.ReferencedMessage;
                if (reply.Embeds.Count > 0)
                {
                    Console.WriteLine(reply.Embeds.Count);
                    Console.WriteLine(reply.Embeds[0].Image.Url.ToString());
                    var link = reply.Embeds[0].Image.Url.ToString();

                    //await ctx.TriggerTypingAsync();

                    var searchResults = await traceDotMoeService.ImageSearch(link);

                    if (searchResults != null)
                    {
                        var english = searchResults.Value<JToken>("anilist").Value<JToken>("title").Value<string>("english");
                        var romaji = searchResults.Value<JToken>("anilist").Value<JToken>("title").Value<string>("romaji");
                        var title = english != null ? english : romaji;
                        var episode = searchResults.Value<int>("episode").ToString();
                        var sceneStart = TimeSpan.FromSeconds(searchResults.Value<double>("from"));
                        var sceneStartString = sceneStart.ToString("g");
                        var mal = searchResults.Value<JToken>("anilist").Value<int>("idMal").ToString();
                        var similarity = searchResults.Value<double>("similarity");

                        await ctx.RespondAsync($"Similarity to image: {similarity:P2}\nTitle: {title}\nEpisode: {episode}\nTimestamp: {sceneStartString}\nTrace.moe URL: https://trace.moe/?url={link} \nMAL: https://myanimelist.net/anime/{mal}");
                    }
                    else
                    {
                        await ctx.RespondAsync($"No results found.");
                    }
                }
                else
                {
                    await ctx.RespondAsync($"No embed found in source message.");
                }
            }
            else
            {
                await ctx.RespondAsync($"Respond to the message containing the image you want to lookup.");
            }

            //Trace.searchResult = null;
        }

        public override async ValueTask AnimeSearch(TextCommandContext ctx, string searchText)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            try
            {
                //var search = ctx.Command.Arguments.ToString();
                // var search = ctx.Message.Content.Replace("!mal ", "");
                // search = search.Replace("!asearch ", "");
                // search = search.Replace("!as ", "");
                // search = search.Replace("!animesearch ", "");
                await ctx.RespondAsync($"https://kitsu.io/anime/{await kitsuService.AnimeSearch(searchText)}");
            }
            catch
            {
                await ctx.RespondAsync($"No anime found.");
            }
        }

        public override async ValueTask MangaSearch(TextCommandContext ctx, string searchText)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            try
            {
                // var search = ctx.Message.Content.Replace("!mangasearch ", "");
                // search = search.Replace("!msearch ", "");
                // search = search.Replace("!ms ", "");
                await ctx.RespondAsync($"https://kitsu.io/manga/{await kitsuService.MangaSearch(searchText)}");
            }
            catch
            {
                await ctx.RespondAsync($"No manga found.");
            }
        }

        public override async ValueTask DiceRoll(TextCommandContext ctx, string diceText)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            try
            {
                var rand = new Random();
                int sum = 0;
                var result = "";

                var num_die = Int32.Parse(diceText.Split("d")[0]);
                var die_sides = Int32.Parse(diceText.Split("d")[1]);
                for (int i = 0; i < num_die; i++)
                {
                    int roll = rand.Next(1, die_sides + 1);
                    sum = sum + roll;
                    if (i + 1 != num_die)
                    {
                        result = result + roll.ToString() + " + ";
                    }
                    else
                    {
                        result = result + roll.ToString() + " = " + sum;
                    }
                }

                if (!string.IsNullOrEmpty(result))
                    result = $"Numbers rolled: {result}";

                if (result.Length > 2000 && result.Length <= 10000)
                {
                    IEnumerable<string> resultChunks = result.Split(1999);
                    var tasks = resultChunks.Select(c => ctx.RespondAsync(c));

                    Parallel.ForEach(tasks, async x => await x);
                }
                else
                {
                    await ctx.RespondAsync(result);
                }
            }
            catch
            {
                await ctx.RespondAsync($"Invalid input.");
            }
        }

        public override async ValueTask CoinFlip(TextCommandContext ctx, string extraText = "")
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            try
            {
                var rand = new Random();
                var result = "";

                if (rand.Next(0, 2) == 1)
                {
                    result = "Heads";
                }
                else
                {
                    result = "Tails";
                }
                System.Console.WriteLine(rand.Next(0, 1));
                await ctx.RespondAsync($"Coin flipped: {result}");
            }
            catch
            {
                await ctx.RespondAsync($"Invalid input.");
            }
        }

        public override async ValueTask SourceCode(TextCommandContext ctx, string extraText = "")
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            await ctx.RespondAsync($"https://bitbucket.org/sean_mahoney/cyber-chan/src/master/");
        }

        public override async ValueTask EightBall(TextCommandContext ctx, string _question)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var responses = new List<string>
                {
                    "It is certain",
                    "It is decidedly so",
                    "Without a doubt",
                    "Yes definitely",
                    "You may rely on it",
                    "As I see it, yes",
                    "Most likely",
                    "Outlook good",
                    "Yes",
                    "Signs point to yes",
                    "Reply hazy try again",
                    "Ask again later",
                    "Better not tell you now",
                    "Cannot predict now",
                    "Concentrate and ask again",
                    "Don't count on it",
                    "My reply is no",
                    "My sources say no",
                    "Outlook not so good",
                    "Very doubtful"
                };

            var random = new Random();
            var embed = new DiscordEmbedBuilder();
            var url = @"https://emojipedia-us.s3.amazonaws.com/thumbs/120/microsoft/135/billiards_1f3b1.png";
            embed.WithThumbnail(url);
            embed.AddField("Question:", _question);
            embed.AddField("Cyber-chan Says:", responses[random.Next(responses.Count)]);

            await ctx.RespondAsync(embed: embed);
        }

        public override async ValueTask GenerateImage(TextCommandContext ctx, string query = "")
        {
            await imageService.GenerateImageCommon(aiService.GenerateImage, ctx, query, "dalle2.png");
        }

        public override async ValueTask GenerateImage2(TextCommandContext ctx, string query = "")
        {
            await imageService.GenerateImageCommon(aiService.GenerateImage2, ctx, query, "dalle3.png");
        }

        public override async ValueTask GPT3Prompt(TextCommandContext ctx, string query = "")
        {
            await ctx.Channel.TriggerTypingAsync();

            if (await aiService.Moderation(query) == "Pass")
            {
                var embed = new DiscordEmbedBuilder();
                embed.AddField("Question:", query);

                foreach (var chunk in (await aiService.GPT3Prompt(query, ctx.User.Mention)).SplitBy(1024))
                {
                    embed.AddField("Cyber-chan Says:", chunk);
                }

                await ctx.RespondAsync(embed);
            }
            else
            {
                await ctx.RespondAsync("Query failed to pass OpenAI content moderation");
            }

        }

        public override async ValueTask ChatGptPrompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT35Prompt, ctx, query);
        }

        public override async ValueTask GPT4Prompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4Prompt, ctx, query);
        }

        public override async ValueTask GPT4PreviewPrompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4PreviewPrompt, ctx, query);
        }

        public override async ValueTask GPT4OmniPrompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4OmniPrompt, ctx, query);
        }
        
        public override async ValueTask GPTO1Prompt(TextCommandContext ctx, string query = "")

        {

            await aiService.GPTPromptCommon(aiService.GPTO1Prompt, ctx, query);

        }
    }
}

