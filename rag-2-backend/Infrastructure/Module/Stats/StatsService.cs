#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.Stats.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Stats;

public class StatsService(IServiceProvider serviceProvider)
{
    private readonly DatabaseContext _context =
        serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();

    private Dictionary<int, GameStatsResponse> CachedGameStats { get; } = new();

    public UserStatsResponse GetStatsForUser(string email, int userId)
    {
        var principal = _context.Users.FirstOrDefault(u => u.Email == email)
                        ?? throw new NotFoundException("User not found");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId)
                   ?? throw new NotFoundException("User not found");

        if (user.Email != email && principal.Role == Role.Student)
            throw new ForbiddenException("Permission denied");

        var records = _context.RecordedGames
            .OrderBy(r => r.Started)
            .Where(r => r.User.Id == userId)
            .Include(recordedGame => recordedGame.Game)
            .ToList();

        return new UserStatsResponse
        {
            FirstPlayed = records.Count > 0 ? records[0].Started : null,
            LastPlayed = records.Count > 0 ? records.Last().Ended : null,
            Games = records.Select(r => r.Game.Id).Distinct().ToList().Count,
            Plays = records.Count,
            TotalStorageMb = GetSizeByUser(userId, 0)
        };
    }

    public GameStatsResponse GetStatsForGame(int gameId)
    {
        var game = _context.Games.FirstOrDefault(g => g.Id == gameId)
                   ?? throw new NotFoundException("Game not found");

        if (CachedGameStats.TryGetValue(game.Id, out var value)
            && value.StatsUpdatedDate.AddDays(1) >= DateTime.Now)
            return CachedGameStats[game.Id];

        return UpdateCachedStats(gameId, game);
    }

    //

    private GameStatsResponse UpdateCachedStats(int gameId, Database.Entity.Game game)
    {
        var records = _context.RecordedGames
            .OrderBy(r => r.Started)
            .Where(r => r.Game.Id == gameId)
            .Include(recordedGame => recordedGame.User)
            .ToList();

        CachedGameStats[game.Id] = new GameStatsResponse
        {
            FirstPlayed = records.Count > 0 ? records[0].Started : null,
            LastPlayed = records.Count > 0 ? records.Last().Ended : null,
            Plays = records.Count,
            TotalStorageMb = GetSizeByGame(gameId, 0),
            TotalPlayers = records.Select(r => r.User.Id).Distinct().Count(),
            StatsUpdatedDate = DateTime.Now
        };

        return CachedGameStats[game.Id];
    }

    private double GetSizeByGame(int gameId, double initialSizeBytes)
    {
        var results = _context.RecordedGames
            .Where(e => e.Game.Id == gameId)
            .Select(e => new
            {
                StringFieldLength = e.Values.Count
            })
            .ToList();

        var totalBytes = results.Sum(r => r.StringFieldLength) + initialSizeBytes;
        return totalBytes / 1024.0;
    }

    private double GetSizeByUser(int userId, double initialSizeBytes)
    {
        var results = _context.RecordedGames
            .Where(e => e.User.Id == userId)
            .Select(e => new
            {
                StringFieldLength = e.Values.Count
            })
            .ToList();

        var totalBytes = results.Sum(r => r.StringFieldLength) + initialSizeBytes;
        return totalBytes / 1024.0;
    }
}