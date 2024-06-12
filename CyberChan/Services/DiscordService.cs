using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyberChan.Services
{
    internal class DiscordService(IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider, DiscordClient discordClient) : IHostedService
    {
        public async Task StartAsync(CancellationToken token)
        {
            discordClient.MessageCreated += Program.AutoReplyToSean;

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
            await discordClient.DisconnectAsync();
            // More cleanup possibly here
        }
    }
}
