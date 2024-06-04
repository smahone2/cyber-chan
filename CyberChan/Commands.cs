using CyberChan.Extensions;
using CyberChan.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using GiphyDotNet.Model.Parameters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GiphyDotNet.Manager;
using TenorSharp;

namespace CyberChan
{
    internal class Commands(Giphy giphy, TenorClient tenorClient, TraceDotMoeService traceDotMoeService, KitsuService kitsuService, AiService aiService, ImageService imageService) : BaseCommandModule
    {
        //test text
        [Command(nameof(Hi))]
        [Description("Just saying hello.")]
        public async Task Hi(CommandContext ctx, [RemainingText] string extraText = "")
        {
            RandomParameter giphyParameters = new RandomParameter()
            {
                Tag = "Hi"
            };
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                ImageUrl = giphy.RandomGif(giphyParameters).Result.Data.EmbedUrl
            };

            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!", embed);
        }
        
        [Command(nameof(Bye))]
        [Description("Just saying goodbye.")]
        public async Task Bye(CommandContext ctx, [RemainingText] string extraText = "")
        {
            RandomParameter giphyParameters = new RandomParameter()
            {
                Tag = "Bye"
            };
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                ImageUrl = giphy.RandomGif(giphyParameters).Result.Data.EmbedUrl
            };
            
            await ctx.RespondAsync($"👋 Bye, {ctx.User.Mention}!", embed);
        }

        [Command(nameof(Waifu))]
        [Description("Let me find your waifu!")]
        public async Task Waifu(CommandContext ctx, [RemainingText] string extraText = "")
        {
            await ctx.TriggerTypingAsync();

            var rand = new Random();
            var search = tenorClient.Search($"Anime Girl", 10, rand.Next(0, 190).ToString());
            var image = search.GifResults[rand.Next(0, 10)];

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                ImageUrl = image.Media[0][TenorSharp.Enums.GifFormat.gif].Url.AbsoluteUri
            };

            await ctx.RespondAsync($"{ctx.User.Mention}, here is your waifu!", embed);
        }

        [Command(nameof(Gif))]
        [Description("Search for any ol' gif! Usage: !gif <search term>")]
        public async Task Gif(CommandContext ctx, [Description("Search term."), RemainingText] string searchText)
        {
            await ctx.TriggerTypingAsync();

            var rand = new Random();
            // var extraText = ctx.Message.Content.Replace("!gif ", "");
            searchText = searchText.Length > 0 ? searchText : "random";
            var search = tenorClient.Search(searchText, 10, rand.Next(0, searchText.Length > 0 ? 50 : 190).ToString());
            var image = search.GifResults[rand.Next(0, 10)];

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                ImageUrl = image.Media[0][TenorSharp.Enums.GifFormat.gif].Url.AbsoluteUri
            };

            await ctx.RespondAsync($"{ctx.User.Mention}, here is your gif!", embed);
        }

        [Command("lookup")]
        [Description("Find anime based on linked image. Usage (reply to message with image): !lookup")]
        [Aliases("find", "get", "trace")]
        public async Task LookupAnime(CommandContext ctx, [RemainingText] string extraText = "")
        {
            var message = ctx.Message;
            if (message.MessageType == MessageType.Reply)
            {
                var reply = message.ReferencedMessage;
                if (reply.Embeds.Count > 0)
                {
                    Console.WriteLine(reply.Embeds.Count);
                    Console.WriteLine(reply.Embeds[0].Image.Url.ToString());
                    var link = reply.Embeds[0].Image.Url.ToString();

                    await ctx.TriggerTypingAsync();

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
                } else
                {
                    await ctx.RespondAsync($"No embed found in source message.");
                }
            } else
            {
                await ctx.RespondAsync($"Respond to the message containing the image you want to lookup.");
            }

            //Trace.searchResult = null;
        }

        //[Command("db")]
        //[Description("Just saying hello.")]
        //public async Task Db(CommandContext ctx)
        //{
        //    Dotabuff(ctx).ConfigureAwait(false).GetAwaiter().GetResult();
        //}

        //[Command("dotabuff")]
        //[Description("Retrieve your most recent dota match from dotabuff.")]
        //[Aliases("db", "dota")]
        //public async Task Dotabuff(CommandContext ctx)
        //{
        //    try
        //    {
        //        if (!Program.steamID.ContainsKey(ctx.User.Username))
        //        {
        //            await ctx.RespondAsync($"Current stored steam id is {Program.steamID[ctx.User.Username]}... Provide a valid steam ID by using command !steamid <steamid>");
        //        }
        //        else
        //        {
        //            await ctx.RespondAsync($"Dotabuff Link: https://www.dotabuff.com/matches/{ Program.dota.GetMatchId(Program.steamID[ctx.User.Username])}");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        await ctx.RespondAsync($"Current stored steam id is {Program.steamID[ctx.User.Username]}... Provide a valid steam ID by using command !steamid <steamid>");
        //    }

        //}

        //[Command("steamname")]
        //[Description("Displays your steam display name.")]
        //public async Task SteamTest(CommandContext ctx)
        //{
        //    try
        //    {
        //        if (!Program.steamID.ContainsKey(ctx.User.Username))
        //        {
        //            await ctx.RespondAsync($"Current stored steam id is {Program.steamID[ctx.User.Username]}... Provide a valid steam ID by using command !steamid <steamid>");
        //        }
        //        else
        //        {
        //            await ctx.RespondAsync($"Steam Display Name: { Program.steam.DisplayNameSearch(Program.steamID[ctx.User.Username])}");
        //        }
        //    }
        //    catch
        //    {
        //        await ctx.RespondAsync($"Current stored steam id is {Program.steamID[ctx.User.Username]}... Provide a valid steam ID by using command !steamid <steamid>");
        //    }

        //}

        //[Command("steamid")]
        //[Description("Associate your Steam ID with the bot. Usage: !steamid <steamid>")]
        //public async Task Steamid(CommandContext ctx)
        //{
        //    var id = ctx.Message.Content.Replace("!steamid ", "");
        //    if (!Program.steamID.ContainsKey(ctx.User.Username))
        //        Program.steamID.Add(ctx.User.Username, id);
        //    else
        //        Program.steamID[ctx.User.Username] = id;
        //    StreamWriter sw = new StreamWriter("SteamIds.txt");
        //    foreach (var user in Program.steamID)
        //    {
        //        sw.WriteLine(user.Key + "," + user.Value);
        //    }
        //    sw.Close();
        //    await ctx.RespondAsync($"Steam Id {id} saved for {ctx.User.Mention}");

        //}

        [Command(nameof(AnimeSearch))]
        [Description("Search Kitsu for an anime. Usage: !animesearch <search term>")]
        [Aliases("as", "mal", "asearch")]
        public async Task AnimeSearch(CommandContext ctx, [Description("Search term."), RemainingText] string searchText)
        {
            try
            {
                //var search = ctx.Command.Arguments.ToString();
                // var search = ctx.Message.Content.Replace("!mal ", "");
                // search = search.Replace("!asearch ", "");
                // search = search.Replace("!as ", "");
                // search = search.Replace("!animesearch ", "");
                await ctx.RespondAsync($"https://kitsu.io/anime/{ kitsuService.AnimeSearch(searchText)}");
            }
            catch
            {
                await ctx.RespondAsync($"No anime found.");
            }
        }

        [Command(nameof(MangaSearch))]
        [Description("Search Kitsu for a manga. Usage: !mangasearch <search term>")]
        [Aliases("ms", "msearch")]
        public async Task MangaSearch(CommandContext ctx, [Description("Search term."), RemainingText] string searchText)
        {
            try
            {
                // var search = ctx.Message.Content.Replace("!mangasearch ", "");
                // search = search.Replace("!msearch ", "");
                // search = search.Replace("!ms ", "");
                await ctx.RespondAsync($"https://kitsu.io/manga/{ kitsuService.MangaSearch(searchText)}");
            }
            catch
            {
                await ctx.RespondAsync($"No manga found.");
            }
        }

        [Command("roll")]
        [Description("Roll some dice. Usage: !roll <[#]d#>")]
        public static async Task DiceRoll(CommandContext ctx, [Description("How many dice and how many faces."), RemainingText] string diceText)
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
                    await Task.WhenAll(tasks);
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

        [Command("flip")]
        [Description("Flip a coin. Usage: !flip")]
        public static async Task CoinFlip(CommandContext ctx, [RemainingText] string extraText = "")
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

        [Command("source")]
        [Description("Link to the source code.")]
        public static async Task SourceCode(CommandContext ctx, [RemainingText] string extraText = "")
        {
            await ctx.RespondAsync($"https://bitbucket.org/sean_mahoney/cyber-chan/src/master/");

        }

        [Command(nameof(EightBall))]
        [Aliases("8ball")]
        [Description("Place important decisions in the hands of RNGesus")]
        public static async Task EightBall(CommandContext ctx, [Description("Question you want Cyber-chan to answer."), RemainingText] string _question)
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

        [Command("dalle2")]
        [Aliases("dalle")]
        [Description("Generate an image with DALL-E. Usage: !dalle2 test")]
        public async Task GenerateImage(CommandContext ctx, [RemainingText] string query = "")
        {
            await imageService.GenerateImageCommon(aiService.GenerateImage, ctx, query, "dalle2.png");
        }


        [Command("dalle3")]
        [Description("Generate an image with DALL-E. Seeds to prevent prompt rewriting are simple and detailed. Seeds to adjust image style are natural and vivid (This is always after a comma). Usage: !dalle3 <simple,natural> test")]
        public async Task GenerateImage2(CommandContext ctx, [RemainingText] string query = "")
        {
            
            await imageService.GenerateImageCommon(aiService.GenerateImage2, ctx, query, "dalle3.png");

        }

        [Command("gpt3")]
        [Aliases("prompt")]
        [Description("Generate text with GPT3. Usage: !gpt3 test")]
        public async Task GPT3Prompt(CommandContext ctx, [RemainingText] string query = "")
        {
            await ctx.TriggerTypingAsync();

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

        [Command("chatgpt")]
        [Aliases("prompt2")]
        [Description("Generate text with ChatGpt. Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !chatgpt <hackerman> test")]
        public async Task ChatGptPrompt(CommandContext ctx, [RemainingText] string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT35Prompt, ctx, query);
        }

        [Command("gpt4")]
        [Aliases("prompt3")]
        [Description("Generate text with GPT4. Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public async Task GPT4Prompt(CommandContext ctx, [RemainingText] string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4Prompt, ctx, query);
        }

        [Command("gpt4p")]
        [Aliases("prompt4", "gpt4preview")]
        [Description("Generate text with GPT4 Preview. Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public async Task GPT4PreviewPrompt(CommandContext ctx, [RemainingText] string query = "")
        {
            await aiService.GPTPromptCommon(aiService.GPT4PreviewPrompt, ctx, query);
        }
    }
}
