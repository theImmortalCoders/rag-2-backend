using rag_2_backend.Models;

namespace rag_2_backend.DTO;

public class GameResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
}