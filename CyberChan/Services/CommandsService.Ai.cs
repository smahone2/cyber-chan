using DSharpPlus.Commands.Processors.TextCommands;
using System.Threading.Tasks;

namespace CyberChan.Services
{
    internal partial class CommandsService
    {
        public override async ValueTask Chat(TextCommandContext ctx, string query = "")
        {
            await aiService.ChatCommandCommon(aiService.Chat, ctx, query);
        }

        public override async ValueTask ChatFast(TextCommandContext ctx, string query = "")
        {
            await aiService.ChatCommandCommon(aiService.ChatFast, ctx, query);
        }

        public override async ValueTask ChatNano(TextCommandContext ctx, string query = "")
        {
            await aiService.ChatCommandCommon(aiService.ChatNano, ctx, query);
        }

        public override async ValueTask Reason(TextCommandContext ctx, string query = "")
        {
            await aiService.ChatCommandCommon(aiService.Reason, ctx, query);
        }

        public override async ValueTask ReasonDeep(TextCommandContext ctx, string query = "")
        {
            await aiService.ChatCommandCommon(aiService.ReasonDeep, ctx, query);
        }

        public override async ValueTask ChatLegacy(TextCommandContext ctx, string query = "")
        {
            await aiService.ChatCommandCommon(aiService.ChatLegacyFlagship, ctx, query);
        }

        public override async ValueTask GenerateImage(TextCommandContext ctx, string query = "")
        {
            await imageService.GenerateImageCommon(aiService.GenerateImage, ctx, query, "image.png");
        }

        public override async ValueTask EditImage(TextCommandContext ctx, string instructions = "")
        {
            await imageService.EditImageFromMessage(ctx, instructions, "edited-image.png");
        }
    }
}
