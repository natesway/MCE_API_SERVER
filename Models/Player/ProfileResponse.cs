using System.Collections.Generic;

namespace MCE_API_SERVER.Models.Player
{
    public class ProfileResponse
    {
        public ProfileResult result { get; set; }
        public object continuationToken { get; set; }
        public object expiration { get; set; }
        public Updates updates { get; set; }

        public ProfileResponse(ProfileData profileData)
        {
            result = ProfileResult.of(profileData);
        }
    }

    public class ProfileResult : ProfileData
    {
        public Dictionary<string, ProfileLevel> levelDistribution { get; set; }

        public static ProfileResult of(ProfileData profileData)
        {
            return new ProfileResult
            {
                totalExperience = profileData.totalExperience,
                level = profileData.level,
                health = profileData.health,
                healthPercentage = profileData.healthPercentage,
                levelDistribution = StateSingleton.levels
            };
        }
    }

    public class ProfileLevel
    {
        public int experienceRequired { get; set; }
        public Rewards rewards { get; set; }

        public ProfileLevel()
        {
            rewards = new Rewards();
        }
    }

    public class ProfileData
    {
        public int totalExperience { get; set; }
        public int level { get; set; }

        public int currentLevelExperience
        {
            get {
                try {
                    if (StateSingleton.levels.TryGetValue(level.ToString(), out ProfileLevel profileLevel)) {
                        return totalExperience - profileLevel.experienceRequired;
                    }
                }
                catch { }

                return totalExperience;
            }
        }

        public int experienceRemaining
        {
            get {
                try {
                    if (StateSingleton.levels.TryGetValue((level + 1).ToString(), out ProfileLevel profileLevel)) {
                        return profileLevel.experienceRequired - currentLevelExperience;
                    }
                }
                catch { }

                return 0;
            }
        }

        public int? health { get; set; }
        public float healthPercentage { get; set; }

        public ProfileData()
        {
            totalExperience = 0;
            level = 1;
            health = 20;
            healthPercentage = 100f;
        }
    }
}
