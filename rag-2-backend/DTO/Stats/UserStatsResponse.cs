namespace rag_2_backend.DTO.Stats;

public class UserStatsResponse
{
    public int Games { get; set; }
    public int Plays { get; set; }
    public double TotalStorageMb { get; set; }
    public DateTime FirstPlayed { get; set; }
    public DateTime LastPlayed { get; set; }
}