namespace rag_2_backend.Infrastructure.Module.Game.Dto;

public class GameResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; set; }
}