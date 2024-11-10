namespace rag_2_backend.Infrastructure.Module.Game.Dto;

public class GameRequest
{
    public required string Name { get; init; }
    public string? Description { get; set; }
}