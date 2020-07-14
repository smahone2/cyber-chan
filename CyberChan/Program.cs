using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using GiphyDotNet.Manager;
using TenorSharp;

namespace CyberChan
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;
        public static Dota dota;
        public static Dictionary<String,String> steamID;
        public static Kitsu kitsu;
        public static Steam steam;
        public static Giphy giphy;
        public static TenorClient tenor;

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
                TokenType = TokenType.Bot
                
            });
            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "!"
            });
            commands.RegisterCommands<Commands>();

            //var tenorConfig = new TenorConfiguration()
            //{
            //    ApiKey = ConfigurationManager.AppSettings["TenorAPI"],
            //    MediaFilter = TenorSharp.Enums.MediaFilter.minimal,
            //    ContentFilter = TenorSharp.Enums.ContentFilter.low
            //};

            dota = new Dota();
            kitsu = new Kitsu();
            steam = new Steam();
            giphy = new Giphy(ConfigurationManager.AppSettings["GiphyAPI"]);
            tenor = new TenorClient(ConfigurationManager.AppSettings["TenorAPI"]);
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

        static async Task AutoReplyToSean(MessageCreateEventArgs e)
        {
            //if (e.Author.Discriminator == "3638") //XPeteX47
            //    await e.Message.RespondAsync("~b-baka!~");
            if (e.Message.Content.ToLower().Contains("anime"))
                await e.Message.RespondAsync("~b-baka!~");
        }


    }
}