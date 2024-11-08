#region

using HttpExceptions.Exceptions;
using Newtonsoft.Json;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.Stats.Dto;
using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;
using Role = rag_2_backend.Infrastructure.Common.Model.Role;

#endregion

namespace rag_2_backend.Infrastructure.Module.Stats;

public class StatsService(
    IConnectionMultiplexer redisConnection,
    UserDao userDao,
    GameDao gameDao,
    GameRecordDao gameRecordDao
)
{
    private const string RedisCacheKeyPrefix = "GameStats:";
    private readonly IDatabase _redisDatabase = redisConnection.GetDatabase();

    public UserStatsResponse GetStatsForUser(string email, int userId)
    {
        var principal = userDao.GetUserByEmailOrThrow(email);
        var user = userDao.GetUserByIdOrThrow(userId);

        if (user.Email != email && principal.Role == Role.Student)
            throw new ForbiddenException("Permission denied");

        var records = gameRecordDao.GetGameRecordsByUserWithGame(userId);

        return new UserStatsResponse
        {
            FirstPlayed = records.Count > 0 ? records[0].Started : null,
            LastPlayed = records.Count > 0 ? records.Last().Ended : null,
            Games = records.Select(r => r.Game.Id).Distinct().ToList().Count,
            Plays = records.Count,
            TotalStorageMb = gameRecordDao.GetGameRecordsByUserWithGame(userId)
                .Select(r => r.SizeMb)
                .ToList()
                .Sum()
        };
    }

    public GameStatsResponse GetStatsForGame(int gameId)
    {
        var game = gameDao.GetGameByIdOrThrow(gameId);

        var cacheKey = $"{RedisCacheKeyPrefix}{game.Id}";
        var cachedStatsJson = _redisDatabase.StringGet(cacheKey);

        if (!string.IsNullOrEmpty(cachedStatsJson))
        {
            var cachedStats = JsonConvert.DeserializeObject<GameStatsResponse>(cachedStatsJson!);
            if (cachedStats.StatsUpdatedDate.AddDays(1) >= DateTime.Now)
                return cachedStats;
        }

        return UpdateCachedStats(gameId, game);
    }

    //

    private GameStatsResponse UpdateCachedStats(int gameId, Database.Entity.Game game)
    {
        var records = gameRecordDao.GetGameRecordsByGameWithUser(gameId);

        var gameStatsResponse = new GameStatsResponse
        {
            FirstPlayed = records.Count > 0 ? records[0].Started : null,
            LastPlayed = records.Count > 0 ? records.Last().Ended : null,
            Plays = records.Count,
            TotalStorageMb = gameRecordDao.GetGameRecordsByGameWithUser(gameId)
                .Select(r => r.SizeMb)
                .ToList()
                .Sum(),
            TotalPlayers = records.Select(r => r.User.Id).Distinct().Count(),
            StatsUpdatedDate = DateTime.Now
        };

        var cacheKey = $"{RedisCacheKeyPrefix}{game.Id}";
        var serializedStats = JsonConvert.SerializeObject(gameStatsResponse);

        _redisDatabase.StringSet(cacheKey, serializedStats, TimeSpan.FromDays(1));

        return gameStatsResponse;
    }
}