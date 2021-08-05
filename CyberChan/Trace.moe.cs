using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Slko.TraceMoeNET;
//using Slko.TraceMoeNET.Models;
using IO.Swagger.Api;
using IO.Swagger.Model;
using Newtonsoft.Json;

namespace CyberChan
{

    class Trace
    {
        public static Result searchResult;
        public static Extra extra;

        public Trace()
        {
        }

        public bool ImageSearch(string imageUrl)
        {
            PerformSearch(imageUrl).ConfigureAwait(true).GetAwaiter().GetResult();
            return searchResult == null ? false : true;
        }

        public class Extra
        {
            public int id;
            public int idMal;
            public Title title;
            public string synonyms;
            public bool isAdult;
        } 

        public class Title
        {
            public string native;
            public string romaji;
            public string english;
        }

        async private Task PerformSearch(string imageUrl)
        {
            //using var client = new TraceMoeClient();
            var client = new DefaultApi();
            var dataList = await client.SearchGetAsync(imageUrl,"");
            //var dataList = await client.SearchByURLAsync(imageUrl);
            dataList.Result.OrderByDescending(item => item.Similarity).ToList();
            foreach (var item in dataList.Result)
            {
                extra = JsonConvert.DeserializeObject<Extra>(item.Anilist);

                if (!extra.isAdult)
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
