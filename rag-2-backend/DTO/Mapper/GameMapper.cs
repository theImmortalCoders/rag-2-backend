namespace rag_2_backend.DTO.Mapper;

using rag_2_backend.DTO;
using rag_2_backend.models.entity;

public class GameMapper
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