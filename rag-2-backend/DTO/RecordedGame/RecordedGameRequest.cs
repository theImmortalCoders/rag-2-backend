using rag_2_backend.Models;

namespace rag_2_backend.DTO.RecordedGame;

public class RecordedGameRequest
{
    public required string GameName { get; init; }
    public required List<RecordedGameValue> Values { get; init; }
}