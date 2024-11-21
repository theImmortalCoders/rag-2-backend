#region

using Newtonsoft.Json;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Stats.Dto;
using StackExchange.Redis;

#endregion

namespace rag_2_backend.Infrastructure.Util;

public class StatsUtil(
    IConfiguration configuration,
    IConnectionMultiplexer redisConnection,
    UserDao userDao,
    GameDao gameDao,
    GameRecordDao gameRecordDao
)
{
    private readonly IDatabase _redisDatabase = redisConnection.GetDatabase();

    public async Task<GameStatsResponse> UpdateCachedGameStats(Game game)
    {
        var records = await gameRecordDao.GetGameRecordsByGameWithUser(game.Id);

        var gameStatsResponse = new GameStatsResponse
        {
            FirstPlayed = records.Count > 0 ? records[0].Started : null,
            LastPlayed = records.Count > 0 ? records.Last().Ended : null,
            Plays = records.Count,
            TotalStorageMb = (await gameRecordDao.GetGameRecordsByGameWithUser(game.Id))
                .Select(r => r.SizeMb)
                .ToList()
                .Sum(),
            TotalPlayers = records.Select(r => r.User.Id).Distinct().Count(),
            StatsUpdatedDate = DateTime.Now
        };

        var cacheKey = $"{configuration.GetValue<string>("Redis:Stats:Prefix")}{game.Id}";
        var serializedStats = JsonConvert.SerializeObject(gameStatsResponse);

        _redisDatabase.StringSet(cacheKey, serializedStats, TimeSpan.FromDays(1));

        return gameStatsResponse;
    }

    public async Task<OverallStatsResponse> UpdateCachedStats()
    {
        var overallStatsResponse = new OverallStatsResponse
        {
            PlayersAmount = await userDao.CountUsers(),
            TotalMemoryMb = await gameRecordDao.CountTotalStorageMb(),
            GamesAmount = (await gameDao.GetAllGames()).Count,
            GameRecordsAmount = await gameRecordDao.CountAllGameRecords(),
            StatsUpdatedDate = DateTime.Now
        };

        var serializedStats = JsonConvert.SerializeObject(overallStatsResponse);
        _redisDatabase.StringSet(
            configuration.GetValue<string>("Redis:Stats:Prefix") +
            configuration.GetValue<string>("Redis:Stats:OverallStatsKey"),
            serializedStats, TimeSpan.FromDays(1));

        return overallStatsResponse;
    }
}