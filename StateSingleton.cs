using MCE_API_SERVER.Models.Features;
using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCE_API_SERVER
{
    public static class StateSingleton
    {
        public static CatalogResponse catalog;
        public static ServerConfig config;
        public static Recipes recipes;
        public static SettingsResponse settings;
        public static ChallengeStorage challengeStorage;
        public static ProductCatalogResponse productCatalog;

        public static Dictionary<string, TappableUtils.TappableLootTable> tappableData { get; set; }

        public static Dictionary<Guid, LocationResponse.ActiveLocationStorage> activeTappables { get; set; }
        public static Dictionary<string, ProfileLevel> levels { get; set; }
        public static Dictionary<Guid, StoreItemInfo> shopItems { get; set; }

        /// <summary>
        /// Represents the server configuration as json, while also having a load method
        /// </summary>
        public class ServerConfig
        {
            //Properties
            public string baseServerIP { get; set; }
            public bool useBaseServerIP { get; set; }
            public string tileServerUrl { get; set; }
            public string playfabTitleId { get; set; }
            public string itemsFolderLocation { get; set; }
            public string efficiencyCategoriesFolderLocation { get; set; }
            public string journalCatalogFileLocation { get; set; }
            public string recipesFileLocation { get; set; }
            public string settingsFileLocation { get; set; }
            public string challengeStorageFolderLocation { get; set; }
            public Guid activeSeasonChallenge { get; set; }
            public string buildplateStorageFolderLocation { get; set; }
            public string sharedBuildplateStorageFolderLocation { get; set; }
            public string productCatalogFileLocation { get; set; }
            public string ShopItemDictionaryFileLocation { get; set; }
            public string LevelDictionaryFileLocation { get; set; }
            public string EncounterLocationsFileLocation { get; set; }
            public Dictionary<string, string> multiplayerAuthKeys { get; set; }
            //tappable settings
            public int minTappableSpawnAmount { get; set; }
            public int maxTappableSpawnAmount { get; set; }
            public double tappableSpawnRadius { get; set; }

            //Load method
            public static ServerConfig getFromFile()
            {
                string file = File.ReadAllText(Util.SavePath_Server + "config/apiconfig.json");
                return JsonConvert.DeserializeObject<ServerConfig>(file);
            }
        }
    }
}
