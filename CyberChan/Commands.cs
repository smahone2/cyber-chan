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
        private const string SeedList = "Available personas: code, hackerman, steve, pirate, uwu, shakespeare.";

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


        // === Chat commands, grouped by usefulness ===

        [Command(nameof(Chat))]
        [TextAlias("chat", "ask", "gpt")]
        [Description("Chat with Cyber-chan using the cheap default model (gpt-5.6-luna) so normal chats stay inexpensive. Use !fast or !thinkdeep for higher-tier models. " + SeedList + " Usage: !chat <hackerman> hello")]
        public abstract ValueTask Chat(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(ChatFast))]
        [TextAlias("fast", "mini")]
        [Description("Chat with a balanced, faster flagship model (gpt-5.6-terra). " + SeedList + " Usage: !fast <hackerman> hello")]
        public abstract ValueTask ChatFast(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(ChatNano))]
        [TextAlias("nano", "quick")]
        [Description("Chat with the smallest / cheapest current-gen model (gpt-5.6-luna). " + SeedList + " Usage: !nano <hackerman> hello")]
        public abstract ValueTask ChatNano(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(Reason))]
        [TextAlias("think", "reason")]
        [Description("Ask a reasoning-focused model (routes to gpt-5.6-terra) for tough or multi-step problems. " + SeedList + " Usage: !think <code> solve...")]
        public abstract ValueTask Reason(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(ReasonDeep))]
        [TextAlias("thinkdeep", "reasondeep", "deepthink")]
        [Description("Ask the highest-capability reasoning model (routes to gpt-5.6-sol). " + SeedList + " Usage: !thinkdeep <code> solve...")]
        public abstract ValueTask ReasonDeep(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(ChatLegacy))]
        [TextAlias("legacy", "gpt55")]
        [Description("Chat using the prior-generation flagship (gpt-5.5). " + SeedList + " Usage: !legacy <hackerman> hello")]
        public abstract ValueTask ChatLegacy(TextCommandContext ctx, [RemainingText] string query = "");


        // === Image commands ===

        [Command(nameof(GenerateImage))]
        [TextAlias("image", "img", "draw", "gptimage")]
        [Description("Generate an image with the current image model (gpt-image-2). Quality defaults to LowQuality. Usage: !image <simple, MediumQuality> prompt")]
        public abstract ValueTask GenerateImage(TextCommandContext ctx, [RemainingText] string query = "");

        [Command(nameof(EditImage))]
        [TextAlias("editimage", "imageedit", "imagevary", "imagemod")]
        [Description("Reply to a message containing an image to edit or create a variation. Usage (reply): !editimage [instructions]")]
        public abstract ValueTask EditImage(TextCommandContext ctx, [RemainingText] string instructions = "");
    }
}
