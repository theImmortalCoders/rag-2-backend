using Microsoft.AspNetCore.Server.IIS;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.models.entity;
using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace rag_2_backend.Services;

public class GameService(DatabaseContext context)
{
    public async Task<IEnumerable<GameResponse>> GetGames()
    {
        var games = await context.Games.ToListAsync();

        return games.Select(g => new GameResponse
        {
            Id = g.Id,
            Name = g.Name,
            GameType = g.GameType
        });
    }

    public void AddGame(GameRequest request)
    {
        if (context.Games.Any(g => g.Name == request.Name))
            throw new BadHttpRequestException("Game with this name already exists");

        var game = new Game
        {
            Name = request.Name,
            GameType = request.GameType
        };

        context.Games.Add(game);
        context.SaveChanges();
    }

    public void EditGame(GameRequest request, int id)
    {
        var game = context.Games.FirstOrDefault(g => g.Id == id) ?? throw new KeyNotFoundException("Game not found");

        if (context.Games.Any(g => g.Name == request.Name && g.Name != game.Name))
            throw new BadHttpRequestException("Game with this name already exists");

        game.Name = request.Name;
        game.GameType = request.GameType;

        context.Games.Update(game);
        context.SaveChanges();
    }

    public void RemoveGame(int id)
    {
        var game = context.Games.SingleOrDefault(g => g.Id == id) ?? throw new KeyNotFoundException("Game not found");

        var records = context.RecordedGames.Where(g => g.Game.Id == id).ToList();
        if (records.Count > 0) throw new BadHttpRequestException("Game has records");

        context.Games.Remove(game);
        context.SaveChanges();
    }
}