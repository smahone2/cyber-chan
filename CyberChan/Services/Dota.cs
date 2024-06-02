using System;
using System.Configuration;
using System.Threading.Tasks;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace CyberChan.Services
{
    class Dota
    {
        //SteamUser config;
        private readonly DOTA2Match match = new(null,null);
        private string matchID = "";

        public Dota()
        {
            //steamweb.
            //match = new DOTA2Match(ConfigurationManager.AppSettings["SteamAPIKey"]);
        }

        public string GetMatchId(string steamid)
        {
            ExecuteGetMatchId(steamid).ConfigureAwait(false).GetAwaiter().GetResult();
            return matchID;
        }

        async private Task ExecuteGetMatchId(string steamid)
        {
            var matches = await match.GetMatchHistoryAsync(null, null, null, null, ulong.Parse(steamid), null, null, "1", null);
            foreach (var item in matches.Data.Matches)
            {
                matchID = item.MatchId.ToString();
            }
        }
    }
}
