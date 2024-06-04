using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace CyberChan.Services
{
    internal class SteamService(SteamUser steamInterface)
    {
        public async Task<string> DisplayNameSearch(string steamid)
        {
            return await PerformDisplayNameSearch(steamid);
        }
        private async Task<string> PerformDisplayNameSearch(string steamid)
        {
            var result = await steamInterface.GetPlayerSummaryAsync(ulong.Parse(steamid));
            return result.Data.Nickname;
            //foreach (var item in matches.Data.Matches)
            //{
            //   matchID = item.MatchId.ToString();
            //}
        }
    }
}
