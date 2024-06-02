using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace CyberChan.Services
{
    class Steam
    {
        private string searchResult = "";
        SteamWebInterfaceFactory webInterfaceFactory;
        SteamUser steamInterface;

        public Steam()
        {
            //match = new DOTA2Match(ConfigurationManager.AppSettings["SteamAPIKey"]);
            webInterfaceFactory = new SteamWebInterfaceFactory(ConfigurationManager.AppSettings["SteamAPIKey"]);
            steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());

        }

        public string DisplayNameSearch(string steamid)
        {
            PerformDisplayNameSearch(steamid).ConfigureAwait(false).GetAwaiter().GetResult();
            return searchResult;
        }

        async private Task PerformDisplayNameSearch(string steamid)
        {
            var result = await steamInterface.GetPlayerSummaryAsync(ulong.Parse(steamid));
            searchResult = result.Data.Nickname;
            //foreach (var item in matches.Data.Matches)
            //{
            //   matchID = item.MatchId.ToString();
            //}
        }
    }
}
