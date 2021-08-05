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
using Steam.Models.SteamEconomy;
using TenorSharp;

namespace CyberChan
{
    public class Commands : BaseCommandModule

    {
        //test text
        [Command("hi")]
        [Description("Just saying hello.")]
        public async Task Hi(CommandContext ctx)
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
        public async Task Bye(CommandContext ctx)
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
        public async Task Waifu(CommandContext ctx)
        {
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
        public async Task Gif(CommandContext ctx)
        {
            var rand = new Random();
            var extraText = ctx.Message.Content.Replace("!gif ", "");
            extraText = extraText.Length > 0 ? extraText : "random";
            var search = Program.tenor.Search(extraText, 10, rand.Next(0, extraText.Length > 0 ? 50 : 1000));
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
        public async Task LookupAnime(CommandContext ctx)
        {
            var message = ctx.Message;
            if (message.MessageType == MessageType.Reply)
            {
                var reply = message.ReferencedMessage;
                if (reply.Embeds.Count > 0)
                {
                    var link = reply.Embeds[0].Url.ToString();

                    if (Program.trace.ImageSearch(link))
                    {
                        var searchResults = Trace.searchResult;
                        var extra = Trace.extra;

                        await ctx.RespondAsync($"Hit");
                        await ctx.RespondAsync(searchResults.Filename);
                        await ctx.RespondAsync(extra.title.native);
                        await ctx.RespondAsync(extra.isAdult.ToString());
                        await ctx.RespondAsync(searchResults.Similarity.ToString());
                        await ctx.RespondAsync(searchResults.Episode);

                        var title = extra.title.english != null ? extra.title.english : extra.title.romaji;
                        var episode = searchResults.Episode.ToString();
                        var sceneStart = TimeSpan.FromSeconds((double)searchResults.From);
                        var sceneStartString = sceneStart.ToString();
                        var mal = extra.idMal.ToString();

                        await ctx.RespondAsync($"Title:{title}\nEpisode:{episode}\nTimestamp:{sceneStartString}\nMAL link:https://myanimelist.net/anime/{mal}");
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
            Trace.extra = null;
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
        public async Task AnimeSearch(CommandContext ctx)
        {
            try
            {
                //var search = ctx.Command.Arguments.ToString();
                var search = ctx.Message.Content.Replace("!mal ", "");
                search = search.Replace("!asearch ", "");
                search = search.Replace("!as ", "");
                search = search.Replace("!animesearch ", "");
                await ctx.RespondAsync($"https://kitsu.io/anime/{ Program.kitsu.AnimeSearch(search)}");
            }
            catch
            {
                await ctx.RespondAsync($"No anime found.");
            }
        }

        [Command("mangasearch")]
        [Description("Search Kitsu for a manga. Usage: !mangasearch <search term>")]
        [Aliases("ms", "msearch")]
        public async Task MangaSearch(CommandContext ctx)
        {
            try
            {
                var search = ctx.Message.Content.Replace("!mangasearch ", "");
                search = search.Replace("!msearch ", "");
                search = search.Replace("!ms ", "");
                await ctx.RespondAsync($"https://kitsu.io/manga/{ Program.kitsu.MangaSearch(search)}");
            }
            catch
            {
                await ctx.RespondAsync($"No manga found.");
            }
        }

        [Command("roll")]
        [Description("Roll some dice. Usage: !roll <[#]d#>")]
        public async Task DiceRoll(CommandContext ctx)
        {
            try
            {
                var rand = new Random();
                int sum = 0;
                var result = "";

                var num_die = Int32.Parse(ctx.Message.Content.Split(" ")[1].Split("d")[0]);
                var die_sides = Int32.Parse(ctx.Message.Content.Split(" ")[1].Split("d")[1]);
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
        public async Task CoinFlip(CommandContext ctx)
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
        public async Task SourceCode(CommandContext ctx)
        {
            await ctx.RespondAsync($"https://bitbucket.org/sean_mahoney/cyber-chan/src/master/");

        }

    }
}
