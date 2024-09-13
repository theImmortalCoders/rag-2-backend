namespace rag_2_backend.DTO.Stats;

public class GameStatsResponse
{
    public int Plays { get; set; }
    public int TotalPlayers { get; set; }
    public double TotalStorageMb { get; set; }
    public DateTime FirstPlayed { get; set; }
    public DateTime LastPlayed { get; set; }
}