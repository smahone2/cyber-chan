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
        public SearchResult searchResult;

        public Trace()
        {
        }

        public bool ImageSearch(string imageUrl)
        {
            PerformSearch(imageUrl).ConfigureAwait(true).GetAwaiter().GetResult();
            return searchResult == null ? false:true;
        }

        async private Task PerformSearch(string imageUrl)
        {
            using var client = new TraceMoeClient();
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
