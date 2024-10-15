namespace rag_2_backend.DTO.Stats;

public class GameStatsResponse
{
    public int Plays { get; init; }
    public int TotalPlayers { get; init; }
    public double TotalStorageMb { get; set; }
    public DateTime FirstPlayed { get; init; }
    public DateTime LastPlayed { get; init; }
    public DateTime StatsUpdatedDate { get; init; }
}