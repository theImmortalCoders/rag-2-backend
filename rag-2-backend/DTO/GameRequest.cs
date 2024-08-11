using rag_2_backend.Models;

namespace rag_2_backend.DTO;

public class GameRequest
{
    public required string Name { get; init; }
    public required GameType GameType { get; init; }
}