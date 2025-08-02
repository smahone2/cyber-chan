using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CyberChan
{
    internal abstract class Commands
    {
        [Command(nameof(Help))]
        [TextAlias("help", "h")]
        [Description("Get Help")]
        public abstract ValueTask Help(TextCommandContext ctx, string command, [RemainingText] string extraText = "");

        [Command(nameof(Hi))]
        [TextAlias("hi")]
        [Description("Just saying hello.")]
        public abstract ValueTask Hi(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(Bye))]
        [TextAlias("bye")]
        [Description("Just saying goodbye.")]
        public abstract ValueTask Bye(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(Waifu))]
        [TextAlias("waifu")]
        [Description("Let me find your waifu!")]
        public abstract ValueTask Waifu(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(Gif))]
        [TextAlias("gif")]
        [Description("Search for any ol' gif! Usage: !gif <search term>")]
        public abstract ValueTask Gif(TextCommandContext ctx, [Description("Search term."), RemainingText] string searchText = null);


        [Command(nameof(LookupAnime))]
        [TextAlias("lookup", "find", "get", "trace")]
        [Description("Find anime based on linked image. Usage (reply to message with image): !lookup")]
        public abstract ValueTask LookupAnime(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(AnimeSearch))]
        [TextAlias("as", "mal", "asearch")]
        [Description("Search Kitsu for an anime. Usage: !animesearch <search term>")]
        public abstract ValueTask AnimeSearch(TextCommandContext ctx, [Description("Search term."), RemainingText] string searchText);


        [Command(nameof(MangaSearch))]
        [TextAlias("ms", "msearch")]
        [Description("Search Kitsu for a manga. Usage: !mangasearch <search term>")]
        public abstract ValueTask MangaSearch(TextCommandContext ctx, [Description("Search term."), RemainingText] string searchText);


        [Command(nameof(DiceRoll))]
        [TextAlias("roll")]
        [Description("Roll some dice. Usage: !roll <[#]d#>")]
        public abstract ValueTask DiceRoll(TextCommandContext ctx, [Description("How many dice and how many faces.")] string diceText);


        [Command(nameof(CoinFlip))]
        [TextAlias("flip")]
        [Description("Flip a coin. Usage: !flip")]
        public abstract ValueTask CoinFlip(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(SourceCode))]
        [TextAlias("source")]
        [Description("Link to the source code.")]
        public abstract ValueTask SourceCode(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(EightBall))]
        [TextAlias("eightball", "8ball")]
        [Description("Place important decisions in the hands of RNGesus")]
        public abstract ValueTask EightBall(TextCommandContext ctx, [Description("Question you want Cyber-chan to answer."), RemainingText] string _question);


        [Command(nameof(GenerateImage))]
        [TextAlias("dalle2", "dalle")]
        [Description("Generate an image with DALL-E. Usage: !dalle2 test")]
        public abstract ValueTask GenerateImage(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GenerateImage2))]
        [TextAlias("dalle3")]
        [Description("Generate an image with DALL-E. Usage: !dalle3 <simple,natural> test")]
        public abstract ValueTask GenerateImage2(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GPT3Prompt))]
        [TextAlias("gpt3", "prompt")]
        [Description("Generate text with GPT3. Usage: !gpt3 test")]
        public abstract ValueTask GPT3Prompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(ChatGptPrompt))]
        [TextAlias("chatgpt", "prompt2")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !chatgpt <hackerman> test")]
        public abstract ValueTask ChatGptPrompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GPT4Prompt))]
        [TextAlias("gpt4", "prompt3")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public abstract ValueTask GPT4Prompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GPT4PreviewPrompt))]
        [TextAlias("gpt4p", "prompt4", "gpt4preview")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public abstract ValueTask GPT4PreviewPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(GPT4OmniPrompt))]
        [TextAlias("gpt4o", "prompt5", "gpt4omni")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public abstract ValueTask GPT4OmniPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(GPTO1Prompt))]
        [TextAlias("gpto1", "prompt6")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gptO1 <hackerman> test")]
        public abstract ValueTask GPTO1Prompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GenerateImageVariation))]
        [TextAlias("dallevary", "imagevary")]
        [Description("Generate variations of an image using DALL-E. Reply to a message with image and use: !dallevary")]
        public abstract ValueTask GenerateImageVariation(TextCommandContext ctx, [RemainingText] string extraText = "");


        //[Command("db")]
        //[Description("Just saying hello.")]
        //public async ValueTask Db(CommandContext ctx)
        //{
        //    Dotabuff(ctx).ConfigureAwait(false).GetAwaiter().GetResult();
        //}

        //[Command("dotabuff")]
        //[Description("Retrieve your most recent dota match from dotabuff.")]
        //[Aliases("db", "dota")]
        //public async ValueTask Dotabuff(CommandContext ctx)
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
        //public async ValueTask SteamTest(CommandContext ctx)
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
        //public async ValueTask Steamid(CommandContext ctx)
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

    }
}
