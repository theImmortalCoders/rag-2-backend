namespace rag_2_backend.DTO;

public class RecordedGameRequest
{
    public required string GameName { get; init; }
    public required List<RecordedGameResponseValue> Values { get; init; }
}