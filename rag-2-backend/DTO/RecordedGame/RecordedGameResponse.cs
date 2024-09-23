#region

using rag_2_backend.Models;

#endregion

namespace rag_2_backend.DTO.RecordedGame;

public class RecordedGameResponse
{
    public int Id { get; set; }
    public List<Player>? Players { get; set; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    public string? OutputSpec { get; set; }
    public object? EndState { get; set; }
}