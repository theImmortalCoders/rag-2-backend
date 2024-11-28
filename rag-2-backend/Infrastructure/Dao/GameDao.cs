#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class GameDao(DatabaseContext dbContext)
{
    public virtual async Task<Game> GetGameByIdOrThrow(int id)
    {
        return await dbContext.Games.SingleOrDefaultAsync(g => g.Id == id) ??
               throw new NotFoundException("Game not found");
    }

    public virtual async Task<Game> GetGameByNameOrThrow(string gameName)
    {
        return await dbContext.Games.SingleOrDefaultAsync(g => Equals(g.Name.ToLower(), gameName.ToLower()))
               ?? throw new NotFoundException("Game not found");
    }

    public virtual async Task<List<Game>> GetAllGames()
    {
        return await dbContext.Games.ToListAsync();
    }
}