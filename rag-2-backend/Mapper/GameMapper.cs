using rag_2_backend.DTO;
using rag_2_backend.DTO.Game;
using rag_2_backend.models.entity;

namespace rag_2_backend.Mapper;

public abstract class GameMapper
{
    public static GameResponse Map(Game game)
    {
        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name
        };
    }
}