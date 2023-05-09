using Newtonsoft.Json;

namespace rache_der_reti.Core.Persistence;

public class Achievements
{
    
    public struct Achievement
    {
        [JsonProperty]
        public readonly string mName;
        [JsonProperty]
        public readonly string mDescription;
        [JsonProperty]
        public int mLevel = 0;

        public Achievement(string name, string description)
        {
            mName = name;
            mDescription = description;
        }
    }

    // Achievements
    [JsonProperty]
    public Achievement mRetiRedemptionAchievement = new("RETI-Redemption", "Win X games");
    [JsonProperty]
    public Achievement mSweaterAchievement = new("Sweater", "Play X Minutes");
    [JsonProperty]
    public Achievement mPacifistAchievement = new("Pacifist", "");
    [JsonProperty]
    public Achievement mLooserAchievement = new("Looser", "");
    [JsonProperty]
    public Achievement mHackerAchievement = new("Hacker", "");

    public void Save()
    {
        // save into file
        Serialize serializer = new Serialize();
        serializer.SerializeObject(this, "Achievements");
    }

    public static Achievements Load()
    {
        Serialize serializer = new Serialize();
        Achievements achievemntsBuilder = new Achievements();

        Achievements achievements = (Achievements)serializer.DeserializeObject(typeof(Achievements), "Achievements") ??
                                    achievemntsBuilder;

        return achievements;
    }

    public void CheckAchievements(Statistics statistics)
    {
        // reti-redemption achievements
        if (statistics.TotalWonGames >= 15)
        {
            mRetiRedemptionAchievement.mLevel = 3;
        }
        else if (statistics.TotalWonGames >= 5)
        {
            mRetiRedemptionAchievement.mLevel = 3;
        }
        else if (statistics.TotalWonGames >= 1)
        {
            mRetiRedemptionAchievement.mLevel = 3;
        }

        // sweater achievements
        if (statistics.TotalGameTimeInSeconds >= 54000)
        {
            mSweaterAchievement.mLevel = 3;
        }
        else if (statistics.TotalGameTimeInSeconds >= 18000)
        {
            mSweaterAchievement.mLevel = 2;
        }
        else if (statistics.TotalGameTimeInSeconds >= 3600)
        {
            mSweaterAchievement.mLevel = 1;
        }
        
        // looser achievement
        if (statistics.TotalLostGames > 10)
        {
            mLooserAchievement.mLevel = 1;
        }
        
        // hackerman achievement
        if (statistics.TotalCollectedCodeSnippets >= 100)
        {
            mHackerAchievement.mLevel = 3;
        }
        else if (statistics.TotalCollectedCodeSnippets >= 50)
        {
            mHackerAchievement.mLevel = 2;
        }
        else if (statistics.TotalCollectedCodeSnippets >= 10)
        {
            mHackerAchievement.mLevel = 1;
        }
    }
}