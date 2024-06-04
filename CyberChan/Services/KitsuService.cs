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
    internal class KitsuService
    {
        private enum SearchType
        {
            Anime = 0,
            Manga = 1
        }

        public async Task<string> AnimeSearch(string animeName)
        {
            return await PerformSearch(animeName, SearchType.Anime);
        }

        public async Task<string> MangaSearch(string mangaName)
        {
            return await PerformSearch(mangaName, SearchType.Manga);
        }

        private async Task<string> PerformSearch(string searchName, SearchType type)
        {
            switch (type)
            {
                case SearchType.Anime:
                    var dataList = await Anime.GetAnimeAsync(searchName);
                    foreach (var item in dataList.Data)
                    {
                        return item.Id;
                    }
                    break;
                case SearchType.Manga:
                    var dataList2 = await Manga.GetMangaAsync(searchName);
                    foreach (var item in dataList2.Data)
                    {
                        return item.Id;
                    }
                    break;
            }

            return string.Empty;
        }
    }
}
