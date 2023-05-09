using rache_der_reti.Core.SoundManagement;

namespace rache_der_reti.Core.Persistence;

public class Persistence
{
    public Settings MySettings { get; }
    public Statistics MyStatistics { get; }
    public Achievements MyAchievements { get; }

    public Persistence()
    {
        MySettings = Settings.Load();
        MySettings.Apply();
        
        MyStatistics = Statistics.Load();

        MyAchievements = Achievements.Load();
    }

    public void Save()
    {
        MySettings.Save();
        MyStatistics.Save();
        MyAchievements.Save();
    }

    public void CheckAchievements()
    {
        MyAchievements.CheckAchievements(MyStatistics);
    }
}