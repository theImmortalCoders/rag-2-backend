#region

using Moq;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Util;
using StackExchange.Redis;
using Xunit;

#endregion

namespace rag_2_backend.Test.Util;

public class StatsUtilTests
{
    private readonly Mock<GameRecordDao> _mockGameRecordDao;
    private readonly Mock<UserDao> _mockUserDao;
    private readonly StatsUtil _statsUtil;

    public StatsUtilTests()
    {
        Mock<IConfiguration> mockConfiguration = new();
        Mock<IConnectionMultiplexer> mockRedisConnection = new();
        Mock<IDatabase> mockRedisDatabase = new();
        _mockUserDao = new Mock<UserDao>(null!);
        _mockGameRecordDao = new Mock<GameRecordDao>(null!);

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(x => x.Value).Returns("game_stats");
        mockConfiguration.Setup(x => x.GetSection("Redis:Stats:Prefix")).Returns(mockSection.Object);
        mockConfiguration.Setup(x => x.GetSection("Redis:Stats:OverallStatsKey")).Returns(mockSection.Object);

        mockRedisConnection.Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockRedisDatabase.Object);

        _statsUtil = new StatsUtil(
            mockConfiguration.Object,
            mockRedisConnection.Object,
            _mockUserDao.Object,
            _mockGameRecordDao.Object
        );
    }

    [Fact]
    public void UpdateCachedGameStats_ShouldCacheAndReturnGameStats()
    {
        var game = new Game
        {
            Id = 1,
            Name = "game1"
        };
        var gameRecords = new List<GameRecord>
        {
            new()
            {
                Started = DateTime.Now.AddDays(-10),
                Ended = DateTime.Now.AddDays(-9),
                SizeMb = 10,
                User = new User
                {
                    Id = 1,
                    Password = null!,
                    Name = null!
                },
                Game = null!,
                Values = null!
            },
            new()
            {
                Started = DateTime.Now.AddDays(-5),
                Ended = DateTime.Now.AddDays(-4),
                SizeMb = 20,
                User = new User
                {
                    Id = 1,
                    Password = null!,
                    Name = null!
                },
                Game = null!,
                Values = null!
            }
        };

        _mockGameRecordDao.Setup(dao => dao.GetGameRecordsByGameWithUser(game.Id)).Returns(gameRecords);

        var result = _statsUtil.UpdateCachedGameStats(game);

        Assert.Equal(gameRecords[0].Started, result.FirstPlayed);
        Assert.Equal(gameRecords[1].Ended, result.LastPlayed);
        Assert.Equal(2, result.Plays);
        Assert.Equal(30, result.TotalStorageMb);
        Assert.Equal(1, result.TotalPlayers);
        Assert.Equal(DateTime.Now.Date, result.StatsUpdatedDate.Date);
    }

    [Fact]
    public void UpdateCachedStats_ShouldCacheAndReturnOverallStats()
    {
        _mockUserDao.Setup(dao => dao.CountUsers()).Returns(100);
        _mockGameRecordDao.Setup(dao => dao.CountTotalStorageMb()).Returns(500);

        var result = _statsUtil.UpdateCachedStats();

        Assert.Equal(100, result.PlayersAmount);
        Assert.Equal(500, result.TotalMemoryMb);
        Assert.Equal(DateTime.Now.Date, result.StatsUpdatedDate.Date);
    }
}