using CyberChan.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CyberChan.Services
{
    internal class DiscordEventHandlerService(AiService aiService, ILogger<DiscordEventHandlerService> logger)
        : IEventHandler<MessageCreatedEventArgs>,
          IEventHandler<ThreadDeletedEventArgs>,
          IEventHandler<ThreadUpdatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
        {
            // Auto-reply logic
            if (eventArgs.Author is { IsBot: false }
                && !string.IsNullOrEmpty(eventArgs.Message?.Content)
                && eventArgs.Message.Content.Contains("anime", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogDebug(
                    "Auto-reply triggered for message {MessageId} in channel {ChannelId}",
                    eventArgs.Message.Id,
                    eventArgs.Channel?.Id);

                await eventArgs.Message.RespondAsync("~b-baka!~");
            }

            // Thread conversation handling
            if (eventArgs.Author == null || eventArgs.Channel == null || eventArgs.Message == null)
                return;

            if (eventArgs.Author.IsBot)
                return;

            if (eventArgs.Channel is not DiscordThreadChannel threadChannel)
                return;

            // If the thread was created by the bot, always reply in it — even if the
            // in-memory conversation state has been lost (e.g. after a restart).
            if (threadChannel.CreatorId == sender.CurrentUser.Id)
            {
                aiService.EnsureThreadConversation(threadChannel.Id);
            }

            if (!aiService.IsThreadConversation(threadChannel.Id))
                return;

            if (string.IsNullOrWhiteSpace(eventArgs.Message.Content))
                return;

            if (eventArgs.Message.Content.StartsWith("!", StringComparison.OrdinalIgnoreCase))
                return;

            await threadChannel.TriggerTypingAsync();

            var result = await aiService.ContinueThreadConversationAsync(
                threadChannel.Id,
                eventArgs.Author.Username,
                eventArgs.Author.Mention,
                eventArgs.Message.Content);

            switch (result.Status)
            {
                case AiService.ConversationResultStatus.Ignored:
                    return;
                case AiService.ConversationResultStatus.ModerationFailed:
                    await threadChannel.SendMessageAsync("⚠️ Sorry, that message didn't pass moderation.");
                    return;
                case AiService.ConversationResultStatus.Success:
                    var responseText = string.IsNullOrWhiteSpace(result.Response)
                        ? "_I'm sorry, I couldn't generate a response._"
                        : result.Response;

                    foreach (var chunk in responseText.SplitBy(1900))
                    {
                        var content = chunk;
                        await threadChannel.SendMessageAsync(content);
                    }
                    return;
            }
        }

        public Task HandleEventAsync(DiscordClient sender, ThreadDeletedEventArgs eventArgs)
        {
            if (eventArgs.Thread != null)
            {
                aiService.ClearConversation(eventArgs.Thread.Id);
            }

            return Task.CompletedTask;
        }

        public Task HandleEventAsync(DiscordClient sender, ThreadUpdatedEventArgs eventArgs)
        {
            if (eventArgs.ThreadBefore != null && eventArgs.ThreadAfter != null &&
                !eventArgs.ThreadBefore.ThreadMetadata.IsArchived && eventArgs.ThreadAfter.ThreadMetadata.IsArchived)
            {
                aiService.ClearConversation(eventArgs.ThreadAfter.Id);
            }

            return Task.CompletedTask;
        }
    }
}
