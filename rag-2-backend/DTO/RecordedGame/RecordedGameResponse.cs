using rag_2_backend.DTO.Game;
using rag_2_backend.DTO.User;
using rag_2_backend.Models;

namespace rag_2_backend.DTO.RecordedGame;

public class RecordedGameResponse
{
    public int Id { get; set; }
    public required UserResponse UserResponse { get; set; }
    public required GameResponse GameResponse { get; set; }
    public required List<RecordedGameValue> Values { get; set; }
    public List<Player>? Players { get; set; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    public string? OutputSpec { get; set; }
    public object? EndState { get; set; }
}