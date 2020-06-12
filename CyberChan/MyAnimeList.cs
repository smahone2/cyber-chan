using System;
using System.Collections.Generic;
using System.Text;
using MyAnimeListSharp.Core;
using MyAnimeListSharp.Auth;
using MyAnimeListSharp.Facade;
using System.Configuration;

namespace CyberChan
{
    class MyAnimeList
    {
        SearchMethods search;
        CredentialContext config;
        //AnimeSearchResponse search;
        private string matchID = "";

        public MyAnimeList()
        {
            config = new CredentialContext();
            config.UserName = ConfigurationManager.AppSettings["MALUsername"];
            config.Password = ConfigurationManager.AppSettings["MALPassword"];
        }

        public int Search(string searchTerm)
        {
            search = new SearchMethods(config);
            var result = search.SearchAnimeDeserialized(searchTerm);
            return result.Entries[0].Id;
        }


    }
}
