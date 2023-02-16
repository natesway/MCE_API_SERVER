using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Player;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MCE_API_SERVER.Utils
{
    public static class ProfileUtils
    {
        public static ProfileData ReadProfile(string playerId)
        {
            return Util.ParseJsonFile<ProfileData>(playerId, "profile");
        }

        public static void AddExperienceToPlayer(string playerId, int experiencePoints)
        {
            var playerProfile = ReadProfile(playerId);
            var currentLvl = playerProfile.level;
            playerProfile.totalExperience += experiencePoints;
            while (currentLvl < 25 && playerProfile.experienceRemaining <= 0) {
                playerProfile.level++;
                RewardLevelupRewards(playerId, playerProfile.level);
            }

            WriteProfile(playerId, playerProfile);
        }

        private static void RewardLevelupRewards(string playerId, int level)
        {
            RewardUtils.RedeemRewards(playerId, StateSingleton.levels[level.ToString()].rewards, EventLocation.LevelUp);
        }

        private static bool WriteProfile(string playerId, ProfileData playerProfile)
        {
            return Util.WriteJsonFile(playerId, playerProfile, "profile");
        }

        public static Dictionary<string, ProfileLevel> readLevelDictionary()
        {
            string filepath = StateSingleton.config.LevelDictionaryFileLocation;
            string levelsJson = Util.LoadSavedServerFileString(filepath);
            return JsonConvert.DeserializeObject<Dictionary<string, ProfileLevel>>(levelsJson);
        }
    }
}
