#region

using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Game.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Common.Mapper;

public abstract class GameMapper
{
    public static GameResponse Map(Game game)
    {
        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            Description = game.Description
        };
    }
}