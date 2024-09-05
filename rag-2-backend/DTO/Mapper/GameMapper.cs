using rag_2_backend.models.entity;

namespace rag_2_backend.DTO.Mapper;

public abstract class GameMapper
{
    public static GameResponse Map(Game game)
    {
        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            GameType = game.GameType
        };
    }
}