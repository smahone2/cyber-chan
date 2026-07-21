using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyberChan.Services
{
    internal class DiscordService(DiscordClient discordClient) : IHostedService
    {
        public async Task StartAsync(CancellationToken token)
        {
            DiscordActivity status = new("Overthrowing the human race", DiscordActivityType.Custom);
            await discordClient.ConnectAsync(status, DiscordUserStatus.Online);
        }

        public async Task StopAsync(CancellationToken token)
        {
            await discordClient.DisconnectAsync();
        }
    }
}
