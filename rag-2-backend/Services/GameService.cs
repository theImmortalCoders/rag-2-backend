using Microsoft.AspNetCore.Server.IIS;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.models.entity;

namespace rag_2_backend.Services;

public class GameService(DatabaseContext _context)
{
    public async Task<IEnumerable<GameResponse>> GetGames()
    {
        var games = await _context.Games.ToListAsync();

        return games.Select(g => new GameResponse
        {
            Id = g.Id,
            Name = g.Name,
            GameType = g.GameType
        });
    }

    public void AddGame(GameRequest request)
    {
        if (_context.Games.Any(g => g.Name == request.Name))
            throw new Microsoft.AspNetCore.Http.BadHttpRequestException("Game already exists");

        var game = new Game
        {
            Name = request.Name,
            GameType = request.GameType
        };

        _context.Games.Add(game);
        _context.SaveChanges();
    }
}