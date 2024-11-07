#region

using HttpExceptions.Exceptions;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class GameDao(DatabaseContext dbContext)
{
    public Game GetGameByIdOrThrow(int id)
    {
        return dbContext.Games.SingleOrDefault(g => g.Id == id) ?? throw new NotFoundException("Game not found");
    }

    public Game GetGameByNameOrThrow(GameRecordRequest recordRequest)
    {
        return dbContext.Games.SingleOrDefault(g => Equals(g.Name.ToLower(), recordRequest.GameName.ToLower()))
               ?? throw new NotFoundException("Game not found");
    }
}