
namespace rache_der_reti.Core.Persistence;

public class Statistics
{
    public int TotalGameTimeInSeconds { get; set; }
    public int TotalPlayedGames { get; set; }
    public int TotalWonGames { get; set; }
    public int TotalLostGames { get; set; }
    public int TotalCollectedCodeSnippets { get; set; }

    public void Save()
    {
        // save into file
        Serialize serializer = new Serialize();
        serializer.SerializeObject(this, "Statistics");
    }

    public static Statistics Load()
    {
        Serialize serializer = new Serialize();
        Statistics statisticsBuilder = new Statistics();
        
        Statistics statistics = (Statistics)serializer.DeserializeObject(typeof(Statistics), "Statistics") ??
                                statisticsBuilder;

        return statistics;
    }

    public string[] ToArray()
    {
        return new[]
        {
            "total game time: " + TotalGameTimeInSeconds,
            "total played games: " + TotalPlayedGames,
            "total won games: " + TotalWonGames,
            "total lost games: " + TotalLostGames,
            "total snippets collected: " + TotalCollectedCodeSnippets
        };
    }
}