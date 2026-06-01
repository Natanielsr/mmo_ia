namespace GameServerApp.Dtos;

public class RankingEntryData
{
    public string Name                { get; set; } = "";
    public int    Level               { get; set; }
    public long   TotalExperience     { get; set; }
    public string JoinedAt            { get; set; } = "";
    public string DiedAt              { get; set; } = "";
    public int    TimeSurvivedSeconds { get; set; }
}
