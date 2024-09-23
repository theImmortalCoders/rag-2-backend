namespace rag_2_backend.DTO.Stats;

public class UserStatsResponse
{
    public int Games { get; init; }
    public int Plays { get; init; }
    public double TotalStorageMb { get; set; }
    public DateTime FirstPlayed { get; init; }
    public DateTime LastPlayed { get; init; }
}