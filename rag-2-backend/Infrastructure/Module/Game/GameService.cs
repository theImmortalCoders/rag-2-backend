#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.Game.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Game;

public class GameService(DatabaseContext context)
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
        var game = context.Games.SingleOrDefault(g => g.Id == id) ?? throw new NotFoundException("Game not found");

        if (context.Games.Any(g => g.Name == request.Name && g.Name != game.Name))
            throw new BadRequestException("Game with this name already exists");

        game.Name = request.Name;

        context.Games.Update(game);
        context.SaveChanges();
    }

    public void RemoveGame(int id)
    {
        var game = GetGameOrThrow(id);

        var records = context.RecordedGames.Where(g => g.Game.Id == id).ToList();
        foreach (var record in records) context.RecordedGames.Remove(record);

        context.Games.Remove(game);
        context.SaveChanges();
    }

    //

    private Database.Entity.Game GetGameOrThrow(int id)
    {
        return context.Games.SingleOrDefault(g => g.Id == id) ?? throw new NotFoundException("Game not found");
    }
}