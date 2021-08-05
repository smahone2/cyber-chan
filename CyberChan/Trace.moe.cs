using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyberChan
{

    class Trace
    {
        public static JToken searchResult;

        public Trace()
        {
        }

        public bool ImageSearch(string imageUrl)
        {
            PerformSearch(imageUrl).ConfigureAwait(false).GetAwaiter().GetResult();
            return searchResult == null ? false : true;
        }

        async private Task PerformSearch(string imageUrl)
        {

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.trace.moe/search?anilistInfo&url={imageUrl}"),
            };
            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var json = JObject.Parse(responseBody);
            foreach (var item in json.Value<JToken>("result"))
            {
                if (!item.Value<JToken>("anilist").Value<bool>("isAdult"))
                {
                    searchResult = item;
                    break;
                } else
                {
                    searchResult = null;
                }

            }
        }
    }
}
