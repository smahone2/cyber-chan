using System;
using System.Collections.Generic;
using System.Text;
using Kitsu;
using System.Configuration;
using Kitsu.Anime;
using System.Threading.Tasks;
using System.ComponentModel.Design;
using Kitsu.Manga;

namespace CyberChan.Services
{
    class Kitsu
    {
        private string searchResult;

        private enum SearchType
        {
            Anime = 0,
            Manga = 1
        }

        public Kitsu()
        {
        }

        public string AnimeSearch(string animeName)
        {
            PerformSearch(animeName, SearchType.Anime).ConfigureAwait(false).GetAwaiter().GetResult();
            return searchResult;
        }

        public string MangaSearch(string mangaName)
        {
            PerformSearch(mangaName, SearchType.Manga).ConfigureAwait(false).GetAwaiter().GetResult();
            return searchResult;
        }

        async private Task PerformSearch(string searchName, SearchType type)
        {
            switch (type)
            {
                case SearchType.Anime:
                    var dataList = await Anime.GetAnimeAsync(searchName);
                    foreach (var item in dataList.Data)
                    {
                        searchResult = item.Id;
                        break;
                    }
                    break;
                case SearchType.Manga:
                    var dataList2 = await Manga.GetMangaAsync(searchName);
                    foreach (var item in dataList2.Data)
                    {
                        searchResult = item.Id;
                        break;
                    }
                    break;
            }


        }

    }
}
