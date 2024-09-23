#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Config;
using rag_2_backend.DTO.Stats;
using rag_2_backend.Models;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.Services;

public class StatsService(DatabaseContext context, UserUtil userUtil)
{
    public UserStatsResponse GetStatsForUser(string email, int userId)
    {
        var user = userUtil.GetUserByEmailOrThrow(email);

        if (user.Email != email && user.Role == Role.Student)
            throw new ForbiddenException("Permission denied");

        var records = context.RecordedGames
            .OrderBy(r => r.Started)
            .Where(r => r.User.Id == userId).Include(recordedGame => recordedGame.Game)
            .ToList();

        return new UserStatsResponse
        {
            FirstPlayed = records[0].Started,
            LastPlayed = records.Last().Ended,
            Games = records.Select(r => r.Game.Id).Distinct().ToList().Count,
            Plays = records.Count,
            TotalStorageMb = GetSizeByUser(userId, 0)
        };
    }

    public GameStatsResponse GetStatsForGame(int gameId)
    {
        var records = context.RecordedGames
            .OrderBy(r => r.Started)
            .Where(r => r.Game.Id == gameId).Include(recordedGame => recordedGame.User)
            .ToList();

        return new GameStatsResponse
        {
            FirstPlayed = records[0].Started,
            LastPlayed = records.Last().Ended,
            Plays = records.Count,
            TotalStorageMb = GetSizeByGame(gameId, 0),
            TotalPlayers = records.Select(r => r.User.Id).Distinct().Count()
        };
    }

    //

    private double GetSizeByGame(int gameId, double initialSizeBytes)
    {
        var results = context.RecordedGames
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
        var results = context.RecordedGames
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