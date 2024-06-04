using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyberChan.Services
{

    internal class TraceDotMoeService(HttpClient httpClient)
    {
        public async Task<JToken> ImageSearch(string imageUrl)
        {
            return await PerformSearch(imageUrl);
        }

        async private Task<JToken> PerformSearch(string imageUrl)
        {
            JToken searchResult = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.trace.moe/search?anilistInfo&url={imageUrl}"),
            };
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var json = JObject.Parse(responseBody);
            foreach (var item in json.Value<JToken>("result"))
            {
                if (!item.Value<JToken>("anilist").Value<bool>("isAdult"))
                {
                    searchResult = item;
                    break;
                }
                else
                {
                    searchResult = null;
                }
            }
            return searchResult;
        }
    }
}
