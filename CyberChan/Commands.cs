using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using Newtonsoft.Json.Linq;
using Steam.Models.SteamEconomy;
using TenorSharp;

namespace CyberChan
{
    public class Commands : BaseCommandModule

    {
        //test text
        [Command("hi")]
        [Description("Just saying hello.")]
        public async Task Hi(CommandContext ctx, string otherText)
        {
            RandomParameter giphyParameters = new RandomParameter()
            {
                Tag = "Hi"
            };
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                ImageUrl = Program.giphy.RandomGif(giphyParameters).Result.Data.ImageUrl
            };

            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!", embed);
        }
        
        [Command("bye")]
        [Description("Just saying goodbye.")]
        public async Task Bye(CommandContext ctx, string otherText)
        {
            RandomParameter giphyParameters = new RandomParameter()
            {
                Tag = "Bye"
            };
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                ImageUrl = Program.giphy.RandomGif(giphyParameters).Result.Data.ImageUrl
            };
            
            await ctx.RespondAsync($"👋 Bye, {ctx.User.Mention}!", embed);
        }

        [Command("waifu")]
        [Description("Let me find your waifu!")]
        public async Task Waifu(CommandContext ctx, string otherText)
        {
            await ctx.TriggerTypingAsync();

            var rand = new Random();
            var search = Program.tenor.Search($"Anime Girl", 10, rand.Next(0,1000));
            var image = search.GifResults[rand.Next(0, 10)];

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                ImageUrl = image.Media[0][TenorSharp.Enums.GifFormat.gif].Url.AbsoluteUri
            };

            await ctx.RespondAsync($"{ctx.User.Mention}, here is your waifu!", embed);
        }

        [Command("gif")]
        [Description("Search for any ol' gif! Usage: !gif <search term>")]
        public async Task Gif(CommandContext ctx, string searchTerm)
        {
            await ctx.TriggerTypingAsync();

            var rand = new Random();
            // var extraText = ctx.Message.Content.Replace("!gif ", "");
            searchTerm = searchTerm.Length > 0 ? searchTerm : "random";
            var search = Program.tenor.Search(searchTerm, 10, rand.Next(0, searchTerm.Length > 0 ? 50 : 1000));
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
        public async Task LookupAnime(CommandContext ctx, string otherText)
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

                    if (Program.trace.ImageSearch(link))
                    {
                        var searchResults = Trace.searchResult;

                        var english = searchResults.Value<JToken>("anilist").Value<JToken>("title").Value<string>("english");
                        var title = english != null ? english : searchResults.Value<JToken>("anilist").Value<JToken>("title").Value<string>("romaji");
                        var episode = searchResults.Value<int>("episode").ToString();
                        var sceneStart = TimeSpan.FromSeconds(searchResults.Value<double>("from"));
                        var sceneStartString = sceneStart.ToString("c");
                        var mal = searchResults.Value<JToken>("anilist").Value<int>("idMal").ToString();
                        var similarity = searchResults.Value<double>("similarity").ToString();

                        await ctx.RespondAsync($"Similarity to image: {similarity}%\nTitle: {title}\nEpisode: {episode}\nTimestamp: {sceneStartString}\nMAL: https://myanimelist.net/anime/{mal}");
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

            Trace.searchResult = null;
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

        [Command("animesearch")]
        [Description("Search Kitsu for an anime. Usage: !animesearch <search term>")]
        [Aliases("as", "mal", "asearch")]
        public async Task AnimeSearch(CommandContext ctx, string searchTerm)
        {
            try
            {
                //var search = ctx.Command.Arguments.ToString();
                // var search = ctx.Message.Content.Replace("!mal ", "");
                // search = search.Replace("!asearch ", "");
                // search = search.Replace("!as ", "");
                // search = search.Replace("!animesearch ", "");
                await ctx.RespondAsync($"https://kitsu.io/anime/{ Program.kitsu.AnimeSearch(searchTerm)}");
            }
            catch
            {
                await ctx.RespondAsync($"No anime found.");
            }
        }

        [Command("mangasearch")]
        [Description("Search Kitsu for a manga. Usage: !mangasearch <search term>")]
        [Aliases("ms", "msearch")]
        public async Task MangaSearch(CommandContext ctx, string searchTerm)
        {
            try
            {
                // var search = ctx.Message.Content.Replace("!mangasearch ", "");
                // search = search.Replace("!msearch ", "");
                // search = search.Replace("!ms ", "");
                await ctx.RespondAsync($"https://kitsu.io/manga/{ Program.kitsu.MangaSearch(searchTerm)}");
            }
            catch
            {
                await ctx.RespondAsync($"No manga found.");
            }
        }

        [Command("roll")]
        [Description("Roll some dice. Usage: !roll <[#]d#>")]
        public async Task DiceRoll(CommandContext ctx, string diceText)
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
        public async Task CoinFlip(CommandContext ctx, string otherText)
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
        public async Task SourceCode(CommandContext ctx, string otherText)
        {
            await ctx.RespondAsync($"https://bitbucket.org/sean_mahoney/cyber-chan/src/master/");

        }

        [Command("eightball")]
        [Aliases("8ball")]
        [Description("Place important decisions in the hands of RNGesus")]
        public async Task EightBall(CommandContext ctx, [Description("Question you want CyberButler to answer."), RemainingText] string _question)
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
            embed.AddField("CyberButler Says:", responses[random.Next(responses.Count)]);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
