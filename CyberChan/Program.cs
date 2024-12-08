using CsvHelper;
using CyberChan.Models;
using CyberChan.Services;
using DSharpPlus;
using DSharpPlus.EventArgs;
using GiphyDotNet.Manager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Betalgo.Ranul.OpenAI.Managers;
using Serilog;
using SteamWebAPI2.Utilities;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Betalgo.Ranul.OpenAI;

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
                        //.ConfigureEventHandlers(b => b.HandleMessageCreated(AutoReplyToSean));
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

        public static async Task AutoReplyToSean(DiscordClient d, MessageCreateEventArgs e)
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
                .AddSingleton(new OpenAIService(new OpenAIOptions { ApiKey = ConfigurationManager.AppSettings["OpenAIAPIKey"] }))
                .AddSingleton(new Giphy(ConfigurationManager.AppSettings["GiphyAPI"]))
                .AddSteamWebInterfaceFactory(x => x.SteamWebApiKey = ConfigurationManager.AppSettings["SteamAPIKey"])
                .AddSingleton<AiService>()
                .AddSingleton<ImageService>()
                .AddSingleton<DotaService>()
                .AddSingleton<KitsuService>()
                .AddSingleton<SteamService>()
                .AddSingleton<TraceDotMoeService>()
                .AddSingleton<CommandsService>()
                .AddHostedService<DiscordService>()
                .AddSingleton(new DiscordClient(new DiscordConfiguration 
                { 
                    TokenType = TokenType.Bot,
                    Token = ConfigurationManager.AppSettings["DiscordToken"], 
                    Intents = DiscordIntents.All 
                }));
        }

        
    }
}