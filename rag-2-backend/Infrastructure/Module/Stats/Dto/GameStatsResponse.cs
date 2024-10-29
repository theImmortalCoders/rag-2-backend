namespace rag_2_backend.Infrastructure.Module.Stats.Dto;

public class GameStatsResponse
{
    public int Plays { get; init; }
    public int TotalPlayers { get; init; }
    public double TotalStorageMb { get; set; }
    public DateTime? FirstPlayed { get; init; }
    public DateTime? LastPlayed { get; init; }
    public DateTime StatsUpdatedDate { get; init; }
}