using CsvHelper;
using CyberChan.Models;
using CyberChan.Services;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using GiphyDotNet.Manager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;
using Serilog;
using SteamWebAPI2.Utilities;
using System;
using System.ClientModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CyberChan
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            GetSteamRecords();

            await Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseConsoleLifetime()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(logging => logging.ClearProviders().AddSerilog())
                        .ConfigureServices();
                })
                .RunConsoleAsync();

            await Log.CloseAndFlushAsync();
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
    }

    public static class ServicesExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddHttpClient()
                .AddSingleton(new OpenAIClient(
                    new ApiKeyCredential(ConfigurationManager.AppSettings["OpenAIAPIKey"] ?? string.Empty),
                    new OpenAIClientOptions
                    {
                        // Image generation (especially at higher qualities) and long chat completions
                        // can exceed the SDK's default network timeout. Allow up to 5 minutes per request.
                        NetworkTimeout = TimeSpan.FromMinutes(5),
                    }))
                .AddSingleton(new Giphy(ConfigurationManager.AppSettings["GiphyAPI"]))
                .AddSteamWebInterfaceFactory(x => x.SteamWebApiKey = ConfigurationManager.AppSettings["SteamAPIKey"])
                .AddSingleton<AiService>()
                .AddSingleton<ImageService>()
                .AddSingleton<DotaService>()
                .AddSingleton<KitsuService>()
                .AddSingleton<SteamService>()
                .AddSingleton<TraceDotMoeService>()
                .AddSingleton<CommandsService>()
                .AddHostedService<DiscordService>();

            var client = DiscordClientBuilder
                .CreateDefault(
                    ConfigurationManager.AppSettings["DiscordToken"] ?? string.Empty,
                    DiscordIntents.All,
                    services)
                .UseCommands(
                    (sp, ext) =>
                    {
                        ext.AddCommands(typeof(CommandsService));
                        ext.AddProcessor(new TextCommandProcessor(new TextCommandConfiguration
                        {
                            PrefixResolver = new DefaultPrefixResolver(false, "!").ResolvePrefixAsync
                        }));
                    },
                    new CommandsConfiguration
                    {
                        DebugGuildId = ulong.Parse(Environment.GetEnvironmentVariable("DEBUG_GUILD_ID") ?? "0")
                    })
                .Build();

            services.AddSingleton(client);

            return services;
        }
    }
}