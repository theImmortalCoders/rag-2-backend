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

    public async Task AddGame(GameRequest request)
    {
        if (await context.Games.AnyAsync(g => g.Name == request.Name))
            throw new BadRequestException("Game with this name already exists");

        var game = new Database.Entity.Game
        {
            Name = request.Name,
            Description = request.Description
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();
    }

    public async Task EditGame(GameRequest request, int id)
    {
        var game = await gameDao.GetGameByIdOrThrow(id);

        if (await context.Games.AnyAsync(g => g.Name == request.Name && g.Name != game.Name))
            throw new BadRequestException("Game with this name already exists");

        game.Name = request.Name;
        game.Description = request.Description;

        context.Games.Update(game);
        await context.SaveChangesAsync();
    }

    public async Task RemoveGame(int id)
    {
        var game = await gameDao.GetGameByIdOrThrow(id);

        var records = await context.GameRecords.Where(g => g.Game.Id == id).ToListAsync();
        foreach (var record in records) context.GameRecords.Remove(record);

        context.Games.Remove(game);
        await context.SaveChangesAsync();
    }
}