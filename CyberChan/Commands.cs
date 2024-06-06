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
        public abstract Task Help(TextCommandContext ctx);

        [Command(nameof(Hi))] 
        [TextAlias("hi")]
        [Description("Just saying hello.")]
        public abstract Task Hi(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(Bye))]
        [TextAlias("bye")]
        [Description("Just saying goodbye.")]
        public abstract Task Bye(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(Waifu))]
        [TextAlias("waifu")]
        [Description("Let me find your waifu!")]
        public abstract Task Waifu(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(Gif))]
        [TextAlias("gif")]
        [Description("Search for any ol' gif! Usage: !gif <search term>")]
        public abstract Task Gif(TextCommandContext ctx, [Description("Search term."), RemainingText] string searchText);


        [Command(nameof(LookupAnime))]
        [TextAlias("lookup", "find", "get", "trace")]
        [Description("Find anime based on linked image. Usage (reply to message with image): !lookup")]
        public abstract Task LookupAnime(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(AnimeSearch))]
        [TextAlias("as", "mal", "asearch")]
        [Description("Search Kitsu for an anime. Usage: !animesearch <search term>")]
        public abstract Task AnimeSearch(TextCommandContext ctx, [Description("Search term."), RemainingText] string searchText);


        [Command(nameof(MangaSearch))]
        [TextAlias("ms", "msearch")]
        [Description("Search Kitsu for a manga. Usage: !mangasearch <search term>")]
        public abstract Task MangaSearch(TextCommandContext ctx, [Description("Search term."), RemainingText] string searchText);


        [Command(nameof(DiceRoll))]
        [TextAlias("roll")]
        [Description("Roll some dice. Usage: !roll <[#]d#>")]
        public abstract Task DiceRoll(TextCommandContext ctx, [Description("How many dice and how many faces.")] string diceText);


        [Command(nameof(CoinFlip))]
        [TextAlias("flip")]
        [Description("Flip a coin. Usage: !flip")]
        public abstract Task CoinFlip(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(SourceCode))]
        [TextAlias("source")]
        [Description("Link to the source code.")]
        public abstract Task SourceCode(TextCommandContext ctx, [RemainingText] string extraText = "");


        [Command(nameof(EightBall))]
        [TextAlias("eightball", "8ball")]
        [Description("Place important decisions in the hands of RNGesus")]
        public abstract Task EightBall(TextCommandContext ctx, [Description("Question you want Cyber-chan to answer."), RemainingText] string _question);


        [Command(nameof(GenerateImage))]
        [TextAlias("dalle2", "dalle")]
        [Description("Generate an image with DALL-E. Usage: !dalle2 test")]
        public abstract Task GenerateImage(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GenerateImage2))]
        [TextAlias("dalle3")]
        [Description("Generate an image with DALL-E. Seeds to prevent prompt rewriting are simple and detailed. Seeds to adjust image style are natural and vivid (This is always after a comma). Usage: !dalle3 <simple,natural> test")]
        public abstract Task GenerateImage2(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GPT3Prompt))]
        [TextAlias("gpt3", "prompt")]
        [Description("Generate text with GPT3. Usage: !gpt3 test")]
        public abstract Task GPT3Prompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(ChatGptPrompt))]
        [TextAlias("chatgpt", "prompt2")]
        [Description("Generate text with ChatGpt. Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !chatgpt <hackerman> test")]
        public abstract Task ChatGptPrompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GPT4Prompt))]
        [TextAlias("gpt4", "prompt3")]
        [Description("Generate text with GPT4. Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public abstract Task GPT4Prompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GPT4PreviewPrompt))]
        [TextAlias("gpt4p", "prompt4", "gpt4preview")]
        [Description("Generate text with GPT4 Preview. Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public abstract Task GPT4PreviewPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(GPT4OmniPrompt))]
        [TextAlias("gpt4o", "prompt5", "gpt4omni")]
        [Description("Generate text with GPT4 Omni. Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !gpt4 <hackerman> test")]
        public abstract Task GPT4OmniPrompt(TextCommandContext ctx, [RemainingText] string query = "");

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

    }
}
