using DSharpPlus;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.EventArgs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CyberChan.Services
{
    internal class DiscordService(IHostApplicationLifetime applicationLifetime, DiscordClient discordClient) : IHostedService
    {
        public async Task StartAsync(CancellationToken token)
        {
            // Add interactivity
            discordClient.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            // Register extensions outside of the service provider lambda since these involve asynchronous operations
            CommandsExtension commandsExtension = discordClient.UseCommands(new CommandsConfiguration()
            {
                DebugGuildId = ulong.Parse(Environment.GetEnvironmentVariable("DEBUG_GUILD_ID") ?? "0"),
                // The default value, however it's shown here for clarity
                RegisterDefaultCommandProcessors = true
            });

            // Add all commands
            commandsExtension.AddCommands(typeof(CommandsService));
            TextCommandProcessor textCommandProcessor = new(new()
            {
                // The default behavior is that the bot reacts to direct mentions
                // and to the "!" prefix.
                // If you want to change it, you first set if the bot should react to mentions
                // and then you can provide as many prefixes as you want.
                PrefixResolver = new DefaultPrefixResolver(true, "!").ResolvePrefixAsync
            });

            // Add text commands with a custom prefix (!)
            await commandsExtension.AddProcessorsAsync(textCommandProcessor);

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
