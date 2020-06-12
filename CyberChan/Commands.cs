using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;

namespace CyberChan
{
    public class Commands
    {
        //test text
        [Command("hi")]
        [Description("Just saying hello.")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!");
        }

        //[Command("db")]
        //[Description("Just saying hello.")]
        //public async Task Db(CommandContext ctx)
        //{
        //    Dotabuff(ctx).ConfigureAwait(false).GetAwaiter().GetResult();
        //}

        [Command("dotabuff")]
        [Description("Retrieve your most recent dota match from dotabuff.")]
        [Aliases("db","dota")]
        public async Task Dotabuff(CommandContext ctx)
        {
            try
            {
                if (!Program.steamID.ContainsKey(ctx.User.Username))
                {
                    await ctx.RespondAsync($"Current stored steam id is {Program.steamID[ctx.User.Username]}... Provide a valid steam ID by using command !steamid <steamid>");
                }
                else
                {
                    await ctx.RespondAsync($"Dotabuff Link: https://www.dotabuff.com/matches/{ Program.dota.GetMatchId(Program.steamID[ctx.User.Username])}");
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"Current stored steam id is {Program.steamID[ctx.User.Username]}... Provide a valid steam ID by using command !steamid <steamid>");
            }

        }

        [Command("steamid")]
        [Description("Associate your Steam ID with the bot. Usage: !steamid <steamid>")]
        public async Task Steamid(CommandContext ctx)
        {
            var id = ctx.Message.Content.Replace("!steamid ", "");
            if (!Program.steamID.ContainsKey(ctx.User.Username))
                Program.steamID.Add(ctx.User.Username, id);
            else
                Program.steamID[ctx.User.Username] = id;
            StreamWriter sw = new StreamWriter("SteamIds.txt");
            foreach (var user in Program.steamID)
            {
                sw.WriteLine(user.Key + "," + user.Value);
            }
            sw.Close();
            await ctx.RespondAsync($"Steam Id {id} saved for {ctx.User.Mention}");

        }

        [Command("mal")]
        [Description("Search MyAnimeList for an anime. Usage: !mal <search term>")]
        public async Task MyAnimeList(CommandContext ctx)
        {
            var search = ctx.Message.Content.Replace("!mal ", "");

            await ctx.RespondAsync($"https://myanimelist.net/anime/{ Program.mal.Search(search)}");

        }

        [Command("source")]
        [Description("Link to the source code.")]
        public async Task SourceCode(CommandContext ctx)
        { 
            await ctx.RespondAsync($"https://bitbucket.org/sean_mahoney/cyber-chan/src/master/");

        }

    }
}
