#region

using HttpExceptions.Exceptions;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class GameDao(DatabaseContext dbContext)
{
    public virtual Game GetGameByIdOrThrow(int id)
    {
        return dbContext.Games.SingleOrDefault(g => g.Id == id) ?? throw new NotFoundException("Game not found");
    }

    public virtual Game GetGameByNameOrThrow(string gameName)
    {
        return dbContext.Games.SingleOrDefault(g => Equals(g.Name.ToLower(), gameName.ToLower()))
               ?? throw new NotFoundException("Game not found");
    }

    public virtual List<Game> GetAllGames()
    {
        return dbContext.Games.ToList();
    }
}