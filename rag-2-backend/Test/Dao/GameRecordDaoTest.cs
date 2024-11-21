#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        var recordsQueryable = records.AsQueryable();
        var gameRecordsDbSetMock = new Mock<DbSet<GameRecord>>();
        gameRecordsDbSetMock.As<IQueryable<GameRecord>>().Setup(m => m.Provider).Returns(recordsQueryable.Provider);
        gameRecordsDbSetMock.As<IQueryable<GameRecord>>().Setup(m => m.Expression).Returns(recordsQueryable.Expression);
        gameRecordsDbSetMock.As<IQueryable<GameRecord>>().Setup(m => m.ElementType)
            .Returns(recordsQueryable.ElementType);
        using var enumerator = recordsQueryable.GetEnumerator();
        gameRecordsDbSetMock.As<IQueryable<GameRecord>>().Setup(m => m.GetEnumerator()).Returns(enumerator);

        _dbContextMock.Setup(db => db.GameRecords).Returns(gameRecordsDbSetMock.Object);
    }

    [Fact]
    public void GetRecordsByGameAndUser_ShouldReturnRecords_WhenRecordsExist()
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

        var result = _gameRecordDao.GetRecordsByGameAndUser(
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
    public void GetGameRecordsByUserWithGame_ShouldReturnRecords_WhenRecordsExist()
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

        var result = _gameRecordDao.GetGameRecordsByUserWithGame(userId);

        Assert.Single(result);
        Assert.Equal(game.Id, result[0].Game.Id);
    }

    [Fact]
    public void GetGameRecordsByGameWithUser_ShouldReturnRecords_WhenRecordsExist()
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

        var result = _gameRecordDao.GetGameRecordsByGameWithUser(gameId);

        Assert.Single(result);
        Assert.Equal(user.Id, result[0].User.Id);
    }

    [Fact]
    public void GetRecordedGameById_ShouldReturnGameRecord_WhenRecordExists()
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

        var result = _gameRecordDao.GetRecordedGameById(recordId);

        Assert.Equal(recordId, result.Id);
    }

    [Fact]
    public void GetRecordedGameById_ShouldThrowNotFoundException_WhenRecordDoesNotExist()
    {
        const int recordId = 1;
        SetUpGameRecordsDbSet(new List<GameRecord>());

        Assert.Throws<NotFoundException>(() => _gameRecordDao.GetRecordedGameById(recordId));
    }
}