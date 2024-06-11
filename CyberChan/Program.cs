using CsvHelper;
using CyberChan.Models;
using CyberChan.Services;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using GiphyDotNet.Manager;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Managers;
using SteamWebAPI2.Utilities;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CyberChan
{
    class Program
    {
        public static DiscordClient _client;

        static async Task Main(string[] args)
        {
            //HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            GetSteamRecords();

            // Configure services
            _client = await ConfigureDiscord();

            await Task.Delay(-1);
        }

        public static void GetSteamRecords()
        {
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
        }

        public static async Task<DiscordClient> ConfigureDiscord()
        {
            var discordClient = DiscordClientBuilder.CreateDefault(ConfigurationManager.AppSettings["DiscordToken"], DiscordIntents.All)
                .ConfigureEventHandlers(x =>
                {
                    x.HandleMessageCreated(Program.AutoReplyToSean);
                })
                .ConfigureServices(services =>
                {
                    services.ConfigureServices();
                })
                .SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
                .Build();

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

            return discordClient;
        }

        public static async Task AutoReplyToSean(DiscordClient d, MessageCreatedEventArgs e)
        {
            //if (e.Author.Discriminator == "3638") //XPeteX47
            //    await e.Message.RespondAsync("~b-baka!~");
            if (e.Message.Content.Contains("anime", StringComparison.OrdinalIgnoreCase))
                await e.Message.RespondAsync("~b-baka!~");
        }
    }

    public static class ServicesExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            return services.AddHttpClient()
                .AddSingleton(new OpenAIService(new OpenAiOptions { ApiKey = ConfigurationManager.AppSettings["OpenAIAPIKey"] }))
                .AddSingleton(new Giphy(ConfigurationManager.AppSettings["GiphyAPI"]))
                .AddSteamWebInterfaceFactory(x => x.SteamWebApiKey = ConfigurationManager.AppSettings["SteamAPIKey"])
                .AddSingleton<AiService>()
                .AddSingleton<ImageService>()
                .AddSingleton<DotaService>()
                .AddSingleton<KitsuService>()
                .AddSingleton<SteamService>()
                .AddSingleton<TraceDotMoeService>()
                .AddSingleton<CommandsService>();
            
            //var tenorConfig = new TenorConfiguration()
            //{
            //    ApiKey = ConfigurationManager.AppSettings["TenorAPI"],
            //    MediaFilter = TenorSharp.Enums.MediaFilter.minimal,
            //    ContentFilter = TenorSharp.Enums.ContentFilter.low
            //};
        }

        
    }
}