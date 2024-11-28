#region

using Microsoft.EntityFrameworkCore;
using Moq;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Stats;
using rag_2_backend.Infrastructure.Module.Stats.Dto;
using rag_2_backend.Infrastructure.Util;
using StackExchange.Redis;
using Xunit;
using Role = rag_2_backend.Infrastructure.Common.Model.Role;

#endregion

namespace rag_2_backend.Test.Service;

public class StatsServiceTests
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly Mock<GameDao> _mockGameDao;
    private readonly Mock<GameRecordDao> _mockGameRecordDao;
    private readonly Mock<UserDao> _mockUserDao;
    private readonly StatsService _statsService;

    public StatsServiceTests()
    {
        Mock<IConnectionMultiplexer> mockRedisConnection = new();
        Mock<IDatabase> mockRedisDatabase = new();
        Mock<IConfiguration> configurationMock = new();
        _mockUserDao = new Mock<UserDao>(_contextMock.Object);
        _mockGameDao = new Mock<GameDao>(_contextMock.Object);
        _mockGameRecordDao = new Mock<GameRecordDao>(_contextMock.Object);
        Mock<StatsUtil> mockStatsUtil = new(configurationMock.Object, mockRedisConnection.Object, _mockUserDao.Object,
            _mockGameDao.Object, _mockGameRecordDao.Object);

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(x => x.Value).Returns("game_stats");
        configurationMock.Setup(x => x.GetSection("Redis:Stats:Prefix")).Returns(mockSection.Object);
        configurationMock.Setup(x => x.GetSection("Redis:Stats:OverallStatsKey")).Returns(mockSection.Object);

        mockRedisConnection.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockRedisDatabase.Object);

        _statsService = new StatsService(
            configurationMock.Object,
            mockRedisConnection.Object,
            _mockUserDao.Object,
            _mockGameDao.Object,
            _mockGameRecordDao.Object,
            mockStatsUtil.Object
        );
    }

    [Fact]
    public async Task GetStatsForUser_ShouldReturnCorrectStats_WhenUserHasPermission()
    {
        const string userEmail = "user@example.com";
        const int userId = 123;
        var principal = new User
        {
            Email = userEmail,
            Role = Role.Student,
            Password = null!,
            Name = null!
        };
        var user = new User
        {
            Email = userEmail,
            Password = null!,
            Name = null!
        };
        var records = new List<GameRecord>
        {
            new()
            {
                Started = DateTime.Now.AddDays(-2),
                Ended = DateTime.Now,
                SizeMb = 150,
                Game = new Game
                {
                    Name = "name"
                },
                User = new User
                {
                    Id = 1,
                    Password = null!,
                    Name = "name"
                },
                Values = null!
            },
            new()
            {
                Started = DateTime.Now.AddDays(-1),
                Ended = DateTime.Now,
                SizeMb = 200,
                Game = new Game
                {
                    Name = "name"
                },
                User = new User
                {
                    Id = 1,
                    Password = null!,
                    Name = "name"
                },
                Values = null!
            }
        };

        _mockUserDao.Setup(u => u.GetUserByEmailOrThrow(userEmail)).ReturnsAsync(principal);
        _mockUserDao.Setup(u => u.GetUserByIdOrThrow(userId)).ReturnsAsync(user);
        _mockGameRecordDao.Setup(gr => gr.GetGameRecordsByUserWithGame(userId)).ReturnsAsync(records);

        var result = await _statsService.GetStatsForUser(userEmail, userId);

        Assert.Equal(1, result.Games);
        Assert.Equal(2, result.Plays);
        Assert.Equal(350, result.TotalStorageMb);
    }

    [Fact]
    public async Task GetStatsForGame_ShouldReturnCachedStats_WhenDataIsInCache()
    {
        const int gameId = 1;
        var game = new Game { Id = gameId, Name = "Game 1" };

        var cachedStats = new GameStatsResponse
        {
            FirstPlayed = DateTime.Now.AddDays(-2),
            LastPlayed = DateTime.Now,
            Plays = 0,
            TotalStorageMb = 0,
            TotalPlayers = 50,
            StatsUpdatedDate = DateTime.Now
        };

        _mockGameDao.Setup(g => g.GetGameByIdOrThrow(gameId)).ReturnsAsync(game);
        _mockGameRecordDao.Setup(dao => dao.GetGameRecordsByGameWithUser(It.IsAny<int>()))
            .ReturnsAsync([]);

        var result = await _statsService.GetStatsForGame(gameId);

        Assert.Equal(cachedStats.Plays, result.Plays);
        Assert.Equal(cachedStats.TotalStorageMb, result.TotalStorageMb);
    }

    [Fact]
    public async Task GetStatsForGame_ShouldUpdateCache_WhenDataIsExpired()
    {
        const int gameId = 1;
        var game = new Game { Id = gameId, Name = "Game 1" };
        var records = new List<GameRecord>
        {
            new()
            {
                Started = DateTime.Now.AddDays(-2),
                Ended = DateTime.Now,
                SizeMb = 150,
                Game = new Game
                {
                    Name = "name"
                },
                User = new User
                {
                    Id = 1,
                    Password = null!,
                    Name = "name"
                },
                Values = null!
            },
            new()
            {
                Started = DateTime.Now.AddDays(-1),
                Ended = DateTime.Now,
                SizeMb = 200,
                Game = new Game
                {
                    Name = "name"
                },
                User = new User
                {
                    Id = 1,
                    Password = null!,
                    Name = "name"
                },
                Values = null!
            }
        };

        var gameStatsResponse = new GameStatsResponse
        {
            FirstPlayed = DateTime.Now.AddDays(-2),
            LastPlayed = DateTime.Now,
            Plays = 2,
            TotalStorageMb = 350,
            TotalPlayers = 1,
            StatsUpdatedDate = DateTime.Now
        };

        _mockGameRecordDao.Setup(gr => gr.GetGameRecordsByGameWithUser(gameId)).ReturnsAsync(records);
        _mockGameDao.Setup(g => g.GetGameByIdOrThrow(gameId)).ReturnsAsync(game);

        var result = await _statsService.GetStatsForGame(gameId);

        Assert.Equal(gameStatsResponse.Plays, result.Plays);
        Assert.Equal(gameStatsResponse.TotalStorageMb, result.TotalStorageMb);
    }
}