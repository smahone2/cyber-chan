using CyberChan.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using GiphyDotNet.Manager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using TenorSharp;

namespace CyberChan
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextExtension commands;
        public static Dota dota;
        public static Dictionary<String,String> steamID;
        public static Services.Kitsu kitsu;
        public static Services.Steam steam;
        public static Giphy giphy;
        public static TenorClient tenor;
        public static Trace trace;
        public static AI aITools;

        static void Main(string[] args)
        {
            String fileName = ConfigurationManager.AppSettings["SteamIDFile"];
   
            // Create Steam ID file if it doesn't exist
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            fs.Close();

            // Load Steam ID File
            StreamReader sr = new StreamReader(fileName);
            steamID = new Dictionary<string, string>();
            while (!sr.EndOfStream)
            {
                string line;
                line = sr.ReadLine();
                steamID.Add(line.Split(",")[0], line.Split(",")[1]);
            }
            sr.Close();

            // Start Main Loop
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            LoadModules();

            Events();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        static void LoadModules()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings["DiscordToken"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All

            });
            
            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "!" }
            });
            commands.RegisterCommands<Commands>();

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            //var tenorConfig = new TenorConfiguration()
            //{
            //    ApiKey = ConfigurationManager.AppSettings["TenorAPI"],
            //    MediaFilter = TenorSharp.Enums.MediaFilter.minimal,
            //    ContentFilter = TenorSharp.Enums.ContentFilter.low
            //};

            dota = new Dota();
            kitsu = new Services.Kitsu();
            steam = new Services.Steam();
            giphy = new Giphy(ConfigurationManager.AppSettings["GiphyAPI"]);
            tenor = new TenorClient(ConfigurationManager.AppSettings["TenorAPI"]);
            trace = new Trace();
            aITools = new AI();
        }

        static void Events()
        {
            //discord.MessageCreated += async e =>
            //{
             //   if (e.Message.Content.ToLower().StartsWith("ping"))
            //        await e.Message.RespondAsync("pong!");
            //};

            discord.MessageCreated += AutoReplyToSean;
        }

        static async Task AutoReplyToSean(DiscordClient d, MessageCreateEventArgs e)
        {
            //if (e.Author.Discriminator == "3638") //XPeteX47
            //    await e.Message.RespondAsync("~b-baka!~");
            if (e.Message.Content.ToLower().Contains("anime"))
                await e.Message.RespondAsync("~b-baka!~");
        }
    }
}