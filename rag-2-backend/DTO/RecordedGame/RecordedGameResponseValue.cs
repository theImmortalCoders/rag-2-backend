namespace rag_2_backend.DTO;

public class RecordedGameResponseValue
{
    public string? Name { get; init; }
    public object? State { get; init; }
    public List<object>? Players { get; init; }
    public string? Timestamp { get; init; }
    public string? OutputSpec { get; init; }
}