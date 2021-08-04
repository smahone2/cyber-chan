using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slko.TraceMoeNET;
using Slko.TraceMoeNET.Models;

namespace CyberChan
{

    class Trace
    {
        private TraceMoeClient client;
        public SearchResult searchResult;

        public Trace()
        {
            client = new TraceMoeClient();
        }

        public bool ImageSearch(string imageUrl)
        {
            PerformSearch(imageUrl).ConfigureAwait(false).GetAwaiter().GetResult();
            return searchResult == null ? false:true;
        }

        async private Task PerformSearch(string imageUrl)
        {
            var dataList = await client.SearchByURLAsync(imageUrl);
            foreach (var item in dataList.Results)
            {
                if (!item.IsAdult)
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
