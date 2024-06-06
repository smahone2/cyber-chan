using AutoMapper.Execution;
using CyberChan.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using GiphyDotNet.Model.Web;
using Newtonsoft.Json.Linq;
using OpenAI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using TenorSharp;

namespace CyberChan.Services
{
    internal class CommandsService(Giphy giphy, TenorClient tenorClient, TraceDotMoeService traceDotMoeService, KitsuService kitsuService, AiService aiService, ImageService imageService) : Commands
    {
        public override async Task Help(TextCommandContext ctx, string command = "", [RemainingText] string extraText = "")
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

        public override async Task Hi(TextCommandContext ctx, string extraText = "")
        {
            RandomParameter giphyParameters = new()
            {
                Tag = "Hi"
            };
            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = giphy.RandomGif(giphyParameters).Result.Data.EmbedUrl
            };

            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!", embed);
        }

        public override async Task Bye(TextCommandContext ctx, string extraText = "")
        {
            RandomParameter giphyParameters = new()
            {
                Tag = "Bye"
            };
            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = giphy.RandomGif(giphyParameters).Result.Data.EmbedUrl
            };

            await ctx.RespondAsync($"👋 Bye, {ctx.User.Mention}!", embed);
        }

        public override async Task Waifu(TextCommandContext ctx, string extraText = "")
        {
            //await ctx.TriggerTypingAsync();

            var rand = new Random();
            var search = tenorClient.Search($"Anime Girl", 10, rand.Next(0, 190).ToString());
            var image = search.GifResults?[rand.Next(0, 10)];

            ArgumentNullException.ThrowIfNull(image);

            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = image.Media[0][TenorSharp.Enums.GifFormat.gif].Url.AbsoluteUri
            };

            await ctx.RespondAsync($"{ctx.User.Mention}, here is your waifu!", embed);
        }

        public override async Task Gif(TextCommandContext ctx, string searchText)
        {
            //await ctx.TriggerTypingAsync();

            var rand = new Random();
            // var extraText = ctx.Message.Content.Replace("!gif ", "");
            searchText = searchText.Length > 0 ? searchText : "random";
            var search = tenorClient.Search(searchText, 10, rand.Next(0, searchText.Length > 0 ? 50 : 190).ToString());
            var image = search.GifResults?[rand.Next(0, 10)];

            ArgumentNullException.ThrowIfNull(image);

            DiscordEmbedBuilder embed = new()
            {
                ImageUrl = image.Media[0][TenorSharp.Enums.GifFormat.gif].Url.AbsoluteUri
            };

            await ctx.RespondAsync($"{ctx.User.Mention}, here is your gif!", embed);
        }

        public override async Task LookupAnime(TextCommandContext ctx, string extraText = "")
        {
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

        public override async Task AnimeSearch(TextCommandContext ctx, string searchText)
        {
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

        public override async Task MangaSearch(TextCommandContext ctx, string searchText)
        {
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

        public override async Task DiceRoll(TextCommandContext ctx, string diceText)
        {
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

        public override async Task CoinFlip(TextCommandContext ctx, string extraText = "")
        {
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

        public override async Task SourceCode(TextCommandContext ctx, string extraText = "")
        {
            await ctx.RespondAsync($"https://bitbucket.org/sean_mahoney/cyber-chan/src/master/");

        }

        public override async Task EightBall(TextCommandContext ctx, string _question)
        {
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

        public override async Task GenerateImage(TextCommandContext ctx, string query = "")
        {
            await imageService.GenerateImageCommon(aiService.GenerateImage, ctx, query, "dalle2.png");
        }

        public override async Task GenerateImage2(TextCommandContext ctx, string query = "")
        {

            await imageService.GenerateImageCommon(aiService.GenerateImage2, ctx, query, "dalle3.png");

        }

        public override async Task GPT3Prompt(TextCommandContext ctx, string query = "")
        {
            //await ctx.TriggerTypingAsync();

            if (await aiService.Moderation(query) == "Pass")
            {
                var embed = new DiscordEmbedBuilder();
                embed.AddField("Question:", query);

                foreach (var chunk in aiService.GPT3Prompt(query, ctx.User.Mention).SplitBy(1024))
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

        public override async Task ChatGptPrompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT35Prompt, ctx, query);
        }

        public override async Task GPT4Prompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4Prompt, ctx, query);
        }

        public override async Task GPT4PreviewPrompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4PreviewPrompt, ctx, query);
        }

        public override async Task GPT4OmniPrompt(TextCommandContext ctx, string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4OmniPrompt, ctx, query);
        }
    }
}

