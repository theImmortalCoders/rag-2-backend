namespace rag_2_backend.Infrastructure.Module.Stats.Dto;

public class OverallStatsResponse
{
    public required int PlayersAmount { get; init; }
    public required double TotalMemoryMb { get; init; }
    public required int GamesAmount { get; init; }
    public required int GameRecordsAmount { get; init; }
    public DateTime StatsUpdatedDate { get; init; }
}