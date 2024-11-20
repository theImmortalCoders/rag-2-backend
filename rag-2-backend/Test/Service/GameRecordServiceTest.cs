#region

using System.Text;
using System.Text.Json;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.GameRecord;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;
using rag_2_backend.Infrastructure.Module.User.Dto;
using Xunit;

#endregion

namespace rag_2_backend.Test.Service;

public class GameRecordServiceTests
{
    private readonly Mock<DatabaseContext> _dbContextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly Mock<GameRecordDao> _gameRecordDaoMock;
    private readonly GameRecordService _gameRecordService;
    private readonly Mock<UserDao> _userDaoMock;

    public GameRecordServiceTests()
    {
        _userDaoMock = new Mock<UserDao>(_dbContextMock.Object);
        _gameRecordDaoMock = new Mock<GameRecordDao>(_dbContextMock.Object);
        Mock<GameDao> gameDaoMock = new(_dbContextMock.Object);
        Mock<IConfiguration> configurationMock = new();

        _gameRecordService = new GameRecordService(
            _dbContextMock.Object,
            configurationMock.Object,
            _userDaoMock.Object,
            _gameRecordDaoMock.Object,
            gameDaoMock.Object
        );
    }

    [Fact]
    public void GetRecordsByGameAndUser_ShouldReturnGameRecords()
    {
        const int gameId = 1;
        const string email = "test@example.com";
        var records = new List<GameRecordResponse>
        {
            new()
            {
                GameName = null!,
                User = new UserResponse
                {
                    Id = 0,
                    Email = null!,
                    Name = null!,
                    StudyCycleYearA = 0,
                    StudyCycleYearB = 0
                }
            }
        };
        var user = new User
        {
            Id = 1,
            Email = email,
            Role = Role.Teacher,
            Password = null!,
            Name = null!
        };
        _userDaoMock.Setup(dao => dao.GetUserByEmailOrThrow(email)).Returns(user);
        _gameRecordDaoMock.Setup(dao => dao.GetRecordsByGameAndUser(gameId, 1)).Returns(records);

        var result = _gameRecordService.GetRecordsByGameAndUser(gameId, 1, email);

        Assert.Equal(records, result);
    }

    [Fact]
    public void DownloadRecordData_ShouldReturnSerializedRecord()
    {
        const int recordedGameId = 1;
        const string email = "test@example.com";
        var user = new User
        {
            Id = 1,
            Email = email,
            Role = Role.Teacher,
            Password = null!,
            Name = null!
        };
        var recordedGame = new GameRecord
        {
            Id = recordedGameId,
            User = user,
            Game = new Game
            {
                Name = "pong"
            },
            Values = []
        };

        _userDaoMock.Setup(dao => dao.GetUserByEmailOrThrow(email)).Returns(user);
        _gameRecordDaoMock.Setup(dao => dao.GetRecordedGameById(recordedGameId)).Returns(recordedGame);

        var result = _gameRecordService.DownloadRecordData(recordedGameId, email);

        Assert.Equal(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(GameRecordMapper.JsonMap(recordedGame))), result);
    }

    [Fact]
    public void DownloadRecordData_ShouldThrowBadRequestException_WhenPermissionDenied()
    {
        const int recordedGameId = 1;
        const string email = "test@example.com";
        var user = new User
        {
            Id = 2,
            Email = email,
            Role = Role.Student,
            Password = null!,
            Name = null!
        };
        var recordedGame = new GameRecord
        {
            Id = recordedGameId,
            User = new User
            {
                Id = 1,
                Password = null!,
                Name = null!
            },
            Game = null!,
            Values = null!
        };

        _userDaoMock.Setup(dao => dao.GetUserByEmailOrThrow(email)).Returns(user);
        _gameRecordDaoMock.Setup(dao => dao.GetRecordedGameById(recordedGameId)).Returns(recordedGame);

        Assert.Throws<ForbiddenException>(() => _gameRecordService.DownloadRecordData(recordedGameId, email));
    }

    [Fact]
    public void RemoveGameRecord_ShouldRemoveRecord_WhenUserHasPermission()
    {
        const int gameRecordId = 1;
        const string email = "test@example.com";
        var user = new User
        {
            Id = 1,
            Email = email,
            Password = null!,
            Name = null!
        };
        var gameRecord = new GameRecord
        {
            Id = gameRecordId,
            User = user,
            Game = null!,
            Values = null!
        };

        _dbContextMock.Setup(ctx => ctx.GameRecords).Returns(() => new List<GameRecord> { gameRecord }
            .AsQueryable().BuildMockDbSet().Object);
        _userDaoMock.Setup(dao => dao.GetUserByEmailOrThrow(email)).Returns(user);
        _gameRecordDaoMock.Setup(dao => dao.GetRecordedGameById(gameRecordId)).Returns(gameRecord);

        _gameRecordService.RemoveGameRecord(gameRecordId, email);

        _dbContextMock.Verify(db => db.GameRecords.Remove(gameRecord), Times.Once);
        _dbContextMock.Verify(db => db.SaveChanges(), Times.Once);
    }

    [Fact]
    public void RemoveGameRecord_ShouldThrowBadRequestException_WhenPermissionDenied()
    {
        const int gameRecordId = 1;
        const string email = "test@example.com";
        var user = new User
        {
            Id = 2,
            Email = email,
            Password = null!,
            Name = null!
        };
        var gameRecord = new GameRecord
        {
            Id = gameRecordId,
            User = new User
            {
                Id = 1,
                Password = null!,
                Name = null!
            },
            Game = null!,
            Values = null!
        };

        _userDaoMock.Setup(dao => dao.GetUserByEmailOrThrow(email)).Returns(user);
        _gameRecordDaoMock.Setup(dao => dao.GetRecordedGameById(gameRecordId)).Returns(gameRecord);

        Assert.Throws<BadRequestException>(() => _gameRecordService.RemoveGameRecord(gameRecordId, email));
    }
}