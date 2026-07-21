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
        [Description("Generate an image with DALL-E. Usage: !dalle3 <simple> test")]
        public abstract ValueTask GenerateImage2(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(GenerateGptImage1))]
        [TextAlias("gptimage", "gptimage1")]
        [Description("Generate an image with GPT-Image-1. Usage: !gptimage1 <simple> test")]
        public abstract ValueTask GenerateGptImage1(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(SimplePrompt))]
        [TextAlias("gpt3", "prompt", "simple")]
        [Description("Generate text with a simple prompt. Usage: !simple test")]
        public abstract ValueTask SimplePrompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(FastPrompt))]
        [TextAlias("chatgpt", "prompt2", "fast")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !fast <hackerman> test")]
        public abstract ValueTask FastPrompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(DeepContextPrompt))]
        [TextAlias("gpt4", "prompt3", "deepcontext")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !deepcontext <hackerman> test")]
        public abstract ValueTask DeepContextPrompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(BalancedPrompt))]
        [TextAlias("gpt4p", "prompt4", "balanced")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !balanced <hackerman> test")]
        public abstract ValueTask BalancedPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(MultimodalPrompt))]
        [TextAlias("gpt4o", "prompt5", "multimodal")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !multimodal <hackerman> test")]
        public abstract ValueTask MultimodalPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(ReasoningPrompt))]
        [TextAlias("gpto1", "prompt6", "reasoning")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !reasoning <hackerman> test")]
        public abstract ValueTask ReasoningPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(FastReasoningPrompt))]
        [TextAlias("o4mini", "prompt7", "fastreasoning")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !fastreasoning <hackerman> test")]
        public abstract ValueTask FastReasoningPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(NanoPrompt))]
        [TextAlias("gpt41nano", "prompt8", "nano")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !nano <hackerman> test")]
        public abstract ValueTask NanoPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(HighQualityPrompt))]
        [TextAlias("gpt41", "prompt9", "highquality")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !highquality <hackerman> test")]
        public abstract ValueTask HighQualityPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(DeepReasoningPrompt))]
        [TextAlias("o3", "prompt10", "deepreasoning")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !deepreasoning <hackerman> test")]
        public abstract ValueTask DeepReasoningPrompt(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(FrontierPrompt))]
        [TextAlias("gpt52", "prompt11", "frontier")]
        [Description("Seeds are hackerman, code, evil, dev, dev+, steve, and dude. Usage: !frontier <hackerman> test")]
        public abstract ValueTask FrontierPrompt(TextCommandContext ctx, [RemainingText] string query = "");


        [Command(nameof(GenerateEditedImage))]
        [TextAlias("image15", "img15", "editedimage")]
        [Description("Generate an edited image with GPT-Image-1. Usage: !editedimage <simple> test")]
        public abstract ValueTask GenerateEditedImage(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(GenerateImageVariation))]
        [TextAlias("editimage", "dallevary", "imagevary", "imagemod")]
        [Description("Modify or create image using GPT Vision + GPT-Image-1. Usage: !editimage [instructions]")]
        public abstract ValueTask GenerateImageVariation(TextCommandContext ctx, [RemainingText] string instructions = "");


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
