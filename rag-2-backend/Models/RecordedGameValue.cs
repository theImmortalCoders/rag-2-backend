namespace rag_2_backend.Models;

public class RecordedGameValue
{
    public string? Name { get; init; }
    public object? State { get; init; }
    public List<Player>? Players { get; init; }
    public string? Timestamp { get; init; }
    public string? OutputSpec { get; init; }
}