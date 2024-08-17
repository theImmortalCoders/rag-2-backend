namespace rag_2_backend.DTO;

public class RecordedGameRequest
{
    public required int GameId { get; init; }
    public required string Value { get; init; }
}