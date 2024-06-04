using CsvHelper;
using CyberChan.Models;
using CyberChan.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using GiphyDotNet.Manager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Managers;
using OpenAI;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using TenorSharp;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using System.Net.Http;

namespace CyberChan
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            // Get Steam Id filename
            string filename = ConfigurationManager.AppSettings["SteamIDFile"];
   
            // Create Steam Id file if it doesn't exist
            using FileStream fs = new(filename, FileMode.OpenOrCreate);

            // Load Steam Id File
            using StreamReader sr = new(fs);
            using CsvReader csv = new(sr, CultureInfo.InvariantCulture);

            // Use Steam Id records for something

            // Get Steam Id records
            csv.GetRecords<SteamId>();

            // Configure host services (dependency injection classes)
            await ConfigureServices(builder.Services);

            // Build host
            using IHost host = builder.Build();

            // Start host
            await host.StartAsync();
        }

        private static async Task ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient()
                .AddSingleton(await ConfigureDiscord())
                .AddSingleton(new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = ConfigurationManager.AppSettings["OpenAIAPIKey"]
                }))
                .AddSingleton(new Giphy(ConfigurationManager.AppSettings["GiphyAPI"]))
                .AddSteamWebInterfaceFactory(x => x.SteamWebApiKey = ConfigurationManager.AppSettings["SteamAPIKey"])
                .AddTransient<AiService>()
                .AddTransient<ImageService>()
                .AddTransient<DotaService>()
                .AddTransient<KitsuService>()
                .AddTransient<SteamService>()
                .AddTransient<TraceDotMoeService>();

            //var tenorConfig = new TenorConfiguration()
            //{
            //    ApiKey = ConfigurationManager.AppSettings["TenorAPI"],
            //    MediaFilter = TenorSharp.Enums.MediaFilter.minimal,
            //    ContentFilter = TenorSharp.Enums.ContentFilter.low
            //};

            services.AddTransient(x => new TenorClient(ConfigurationManager.AppSettings["TenorAPI"]));
        }

        private async static Task<DiscordClient> ConfigureDiscord()
        {
            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings["DiscordToken"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = ["!"]
            });
            commands.RegisterCommands<Commands>();

            discord.MessageCreated += AutoReplyToSean;

            await discord.ConnectAsync();

            return discord;
        }

        static async Task AutoReplyToSean(DiscordClient d, MessageCreateEventArgs e)
        {
            //if (e.Author.Discriminator == "3638") //XPeteX47
            //    await e.Message.RespondAsync("~b-baka!~");
            if (e.Message.Content.Contains("anime", StringComparison.OrdinalIgnoreCase))
                await e.Message.RespondAsync("~b-baka!~");
        }
    }
}