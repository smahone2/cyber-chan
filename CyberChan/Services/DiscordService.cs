using CyberChan.Extensions;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CyberChan.Services
{
    internal class DiscordService(IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider, DiscordClient discordClient) : IHostedService
    {
        private static readonly ConcurrentDictionary<(Type Type, string Name), PropertyInfo> PropertyCache = new();
        private readonly AiService aiService = serviceProvider.GetRequiredService<AiService>();

        public async Task StartAsync(CancellationToken token)
        {
            discordClient.MessageCreated += Program.AutoReplyToSean;
            discordClient.MessageCreated += HandleThreadConversationAsync;
            discordClient.ThreadDeleted += HandleThreadDeletedAsync;
            discordClient.ThreadUpdated += HandleThreadUpdatedAsync;

            // Add interactivity
            discordClient.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            // Register extensions outside of the service provider lambda since these involve asynchronous operations
            CommandsExtension commandsExtension = discordClient.UseCommands(new CommandsConfiguration
            {
                ServiceProvider = serviceProvider,
                DebugGuildId = ulong.Parse(Environment.GetEnvironmentVariable("DEBUG_GUILD_ID") ?? "0")
            });

            // Add all commands
            commandsExtension.AddCommands(typeof(CommandsService));
            TextCommandProcessor textCommandProcessor = new(new()
            {
                // The default behavior is that the bot reacts to direct mentions
                // and to the "!" prefix.
                // If you want to change it, you first set if the bot should react to mentions
                // and then you can provide as many prefixes as you want.
                PrefixResolver = new DefaultPrefixResolver("!").ResolvePrefixAsync
            });

            // Add text commands with a custom prefix (!)
            await commandsExtension.AddProcessorAsync(textCommandProcessor);

            // We can specify a status for our bot. Let's set it to "playing" and set the activity to "with fire".
            DiscordActivity status = new("Overthrowing the human race", DiscordActivityType.Custom);

            // Now we connect and log in.
            await discordClient.ConnectAsync(status, DiscordUserStatus.Online);
        }

        public async Task StopAsync(CancellationToken token)
        {
            discordClient.MessageCreated -= Program.AutoReplyToSean;
            discordClient.MessageCreated -= HandleThreadConversationAsync;
            discordClient.ThreadDeleted -= HandleThreadDeletedAsync;
            discordClient.ThreadUpdated -= HandleThreadUpdatedAsync;
            await discordClient.DisconnectAsync();
            // More cleanup possibly here
        }

        private async Task HandleThreadConversationAsync(DiscordClient sender, object e)
        {
            var author = GetPropertyValue<DiscordUser>(e, "Author");
            var channel = GetPropertyValue<DiscordChannel>(e, "Channel");
            var message = GetPropertyValue<DiscordMessage>(e, "Message");

            if (author == null || channel == null || message == null)
            {
                return;
            }

            if (author.IsBot)
            {
                return;
            }

            if (channel is not DiscordThreadChannel threadChannel)
            {
                return;
            }

            if (!aiService.IsThreadConversation(threadChannel.Id))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Content))
            {
                return;
            }

            if (message.Content.StartsWith("!", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await threadChannel.TriggerTypingAsync();

            var result = await aiService.ContinueThreadConversationAsync(threadChannel.Id, author.Username, author.Mention, message.Content);

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

                    var firstChunk = true;
                    foreach (var chunk in responseText.SplitBy(1900))
                    {
                        var content = firstChunk ? $"**Cyber-chan:**\n{chunk}" : chunk;
                        await threadChannel.SendMessageAsync(content);
                        firstChunk = false;
                    }

                    return;
            }
        }

        private Task HandleThreadDeletedAsync(DiscordClient sender, object e)
        {
            var thread = GetPropertyValue<DiscordThreadChannel>(e, "Thread");
            if (thread != null)
            {
                aiService.ClearConversation(thread.Id);
            }

            return Task.CompletedTask;
        }

        private Task HandleThreadUpdatedAsync(DiscordClient sender, object e)
        {
            var threadBefore = GetPropertyValue<DiscordThreadChannel>(e, "ThreadBefore");
            var threadAfter = GetPropertyValue<DiscordThreadChannel>(e, "ThreadAfter");

            // Only clean up if the thread was just archived (wasn't archived before, but is now)
            if (threadBefore != null && threadAfter != null &&
                !threadBefore.ThreadMetadata.IsArchived && threadAfter.ThreadMetadata.IsArchived)
            {
                aiService.ClearConversation(threadAfter.Id);
            }

            return Task.CompletedTask;
        }

        private static T GetPropertyValue<T>(object obj, string propertyName) where T : class
        {
            var key = (obj.GetType(), propertyName);
            var property = PropertyCache.GetOrAdd(key, static item => item.Type.GetProperty(item.Name));
            return property?.GetValue(obj) as T;
        }
    }
}
