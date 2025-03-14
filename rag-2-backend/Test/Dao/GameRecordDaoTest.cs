#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;
using Xunit;

#endregion

namespace rag_2_backend.Test.Dao;

public class GameRecordDaoTests
{
    private readonly Mock<DatabaseContext> _dbContextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly GameRecordDao _gameRecordDao;

    public GameRecordDaoTests()
    {
        _gameRecordDao = new GameRecordDao(_dbContextMock.Object);
    }

    private void SetUpGameRecordsDbSet(IEnumerable<GameRecord> records)
    {
        _dbContextMock.Setup(db => db.GameRecords).ReturnsDbSet(records);
    }

    [Fact]
    public async Task GetRecordsByGameAndUser_ShouldReturnRecords_WhenRecordsExist()
    {
        const int gameId = 1;
        const string email = "test@example.com";
        var game = new Game
        {
            Id = gameId,
            Name = null!
        };
        var user = new User
        {
            Id = 1,
            Email = email,
            Password = null!,
            Name = null!
        };
        var gameRecord = new GameRecord
        {
            Game = game,
            User = user,
            Values = null!
        };
        SetUpGameRecordsDbSet(new List<GameRecord> { gameRecord });

        var result = await _gameRecordDao.GetRecordsByGameAndUser(
            gameId,
            1,
            null,
            null,
            null, SortDirection.Asc,
            GameRecordSortByFields.Id
        );

        Assert.Single(result);
    }

    [Fact]
    public async Task GetGameRecordsByUserWithGame_ShouldReturnRecords_WhenRecordsExist()
    {
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            Password = null!,
            Name = null!
        };
        var game = new Game
        {
            Id = 1,
            Name = null!
        };
        var gameRecord = new GameRecord
        {
            Game = game,
            User = user,
            Values = null!
        };
        SetUpGameRecordsDbSet(new List<GameRecord> { gameRecord });

        var result = await _gameRecordDao.GetGameRecordsByUserWithGame(userId);

        Assert.Single(result);
        Assert.Equal(game.Id, result[0].Game.Id);
    }

    [Fact]
    public async Task GetGameRecordsByGameWithUser_ShouldReturnRecords_WhenRecordsExist()
    {
        const int gameId = 1;
        var game = new Game
        {
            Id = gameId,
            Name = null!
        };
        var user = new User
        {
            Id = 1,
            Password = null!,
            Name = null!
        };
        var gameRecord = new GameRecord
        {
            Game = game,
            User = user,
            Values = null!
        };
        SetUpGameRecordsDbSet(new List<GameRecord> { gameRecord });

        var result = await _gameRecordDao.GetGameRecordsByGameWithUser(gameId);

        Assert.Single(result);
        Assert.Equal(user.Id, result[0].User.Id);
    }

    [Fact]
    public async Task GetRecordedGameById_ShouldReturnGameRecord_WhenRecordExists()
    {
        const int recordId = 1;
        var gameRecord = new GameRecord
        {
            Id = recordId,
            Game = null!,
            User = null!,
            Values = null!
        };
        SetUpGameRecordsDbSet(new List<GameRecord> { gameRecord });

        var result = await _gameRecordDao.GetRecordedGameById(recordId);

        Assert.Equal(recordId, result.Id);
    }

    [Fact]
    public async Task GetRecordedGameById_ShouldThrowNotFoundException_WhenRecordDoesNotExist()
    {
        const int recordId = 1;
        SetUpGameRecordsDbSet(new List<GameRecord>());

        await Assert.ThrowsAsync<NotFoundException>(() => _gameRecordDao.GetRecordedGameById(recordId));
    }
}