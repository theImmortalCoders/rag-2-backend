#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.Game.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Game;

public class GameService(DatabaseContext context, GameDao gameDao)
{
    public async Task<IEnumerable<GameResponse>> GetGames()
    {
        var games = await context.Games.ToListAsync();

        return games.Select(GameMapper.Map);
    }

    public void AddGame(GameRequest request)
    {
        if (context.Games.Any(g => g.Name == request.Name))
            throw new BadRequestException("Game with this name already exists");

        var game = new Database.Entity.Game
        {
            Name = request.Name
        };

        context.Games.Add(game);
        context.SaveChanges();
    }

    public void EditGame(GameRequest request, int id)
    {
        var game = gameDao.GetGameByIdOrThrow(id);

        if (context.Games.Any(g => g.Name == request.Name && g.Name != game.Name))
            throw new BadRequestException("Game with this name already exists");

        game.Name = request.Name;

        context.Games.Update(game);
        context.SaveChanges();
    }

    public void RemoveGame(int id)
    {
        var game = gameDao.GetGameByIdOrThrow(id);

        var records = context.GameRecords.Where(g => g.Game.Id == id).ToList();
        foreach (var record in records) context.GameRecords.Remove(record);

        context.Games.Remove(game);
        context.SaveChanges();
    }
}