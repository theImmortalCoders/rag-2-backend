#region

using HttpExceptions.Exceptions;
using Newtonsoft.Json;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.Stats.Dto;
using rag_2_backend.Infrastructure.Util;
using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;
using Role = rag_2_backend.Infrastructure.Common.Model.Role;

#endregion

namespace rag_2_backend.Infrastructure.Module.Stats;

public class StatsService(
    IConfiguration configuration,
    IConnectionMultiplexer redisConnection,
    UserDao userDao,
    GameDao gameDao,
    GameRecordDao gameRecordDao,
    StatsUtil statsUtil
)
{
    private readonly IDatabase _redisDatabase = redisConnection.GetDatabase();

    public async Task<UserStatsResponse> GetStatsForUser(string email, int userId)
    {
        var principal = await userDao.GetUserByEmailOrThrow(email);
        var user = await userDao.GetUserByIdOrThrow(userId);

        if (user.Email != email && principal.Role == Role.Student)
            throw new ForbiddenException("Permission denied");

        var records = await gameRecordDao.GetGameRecordsByUserWithGame(userId);

        return new UserStatsResponse
        {
            FirstPlayed = records.Count > 0 ? records[0].Started : null,
            LastPlayed = records.Count > 0 ? records.Last().Ended : null,
            Games = records.Select(r => r.Game.Id).Distinct().ToList().Count,
            Plays = records.Count,
            TotalStorageMb = (await gameRecordDao.GetGameRecordsByUserWithGame(userId))
                .Select(r => r.SizeMb)
                .ToList()
                .Sum()
        };
    }

    public async Task<GameStatsResponse> GetStatsForGame(int gameId)
    {
        var game = await gameDao.GetGameByIdOrThrow(gameId);

        var cacheKey = $"{configuration.GetValue<string>("Redis:Stats:Prefix")}{game.Id}";
        var cachedStatsJson = _redisDatabase.StringGet(cacheKey);

        return !string.IsNullOrEmpty(cachedStatsJson)
            ? JsonConvert.DeserializeObject<GameStatsResponse>(cachedStatsJson!)
            : await statsUtil.UpdateCachedGameStats(game);
    }

    public async Task<OverallStatsResponse> GetOverallStats()
    {
        var cachedStatsJson = _redisDatabase.StringGet(
            configuration.GetValue<string>("Redis:Stats:Prefix") +
            configuration.GetValue<string>("Redis:Stats:OverallStatsKey")
        );

        return !string.IsNullOrEmpty(cachedStatsJson)
            ? JsonConvert.DeserializeObject<OverallStatsResponse>(cachedStatsJson!)
            : await statsUtil.UpdateCachedStats();
    }
}