using System;
using System.Configuration;
using System.Threading.Tasks;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace CyberChan.Services
{
    internal class DotaService
    {
        public async Task<string> GetMatchId(string steamid)
        {
            return await ExecuteGetMatchId(steamid);
        }

        private async Task<string> ExecuteGetMatchId(string steamid)
        {
            DOTA2Match match = new(null, null);
            var matches = await match.GetMatchHistoryAsync(null, null, null, null, ulong.Parse(steamid), null, null, "1", null);

            var matchId = string.Empty;
            foreach (var item in matches.Data.Matches)
            {
                matchId = item.MatchId.ToString();
            }

            return matchId;
        }
    }
}
