#region

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.Config;
using rag_2_backend.DTO.RecordedGame;
using rag_2_backend.Models;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;
using rag_2_backend.Services;
using rag_2_backend.Utils;
using Xunit;

#endregion

namespace rag_2_backend.Test;

public class GameRecordServiceTest
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly Game _game = new()
    {
        Id = 1,
        Name = "pong"
    };

    private readonly GameRecordService _gameRecordService;

    private readonly List<RecordedGame> _recordedGames = [];

    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Name = "John",
        Password = "password",
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023
    };

    public GameRecordServiceTest()
    {
        _contextMock.Setup(c => c.RecordedGames).Returns(
            _recordedGames.AsQueryable().BuildMockDbSet().Object
        );
        _contextMock.Setup(c => c.Users).Returns(
            new List<User> { _user }.AsQueryable().BuildMockDbSet().Object
        );
        _contextMock.Setup(c => c.Games).Returns(
            new List<Game> { _game }.AsQueryable().BuildMockDbSet().Object
        );
        Mock<UserUtil> userMock = new(_contextMock.Object);

        userMock.Setup(u => u.GetUserByIdOrThrow(It.IsAny<int>())).Returns(_user);
        userMock.Setup(u => u.GetUserByEmailOrThrow(It.IsAny<string>())).Returns(_user);

        var inMemorySettings = new Dictionary<string, string>
        {
            { "UserDataLimitMb", "10" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _gameRecordService = new GameRecordService(_contextMock.Object, configuration, userMock.Object);

        _recordedGames.Add(new RecordedGame
        {
            Id = 1,
            Game = _game,
            Values =
            [
                new RecordedGameValue()
            ],
            User = _user
        });
    }

    [Fact]
    public void GetRecordsByGameTest()
    {
        var actualRecords = _gameRecordService.GetRecordsByGameAndUser(1, "email@prz.edu.pl");
        List<RecordedGameResponse> expectedRecords =
        [
            new()
            {
                Id = 1
            }
        ];

        Assert.Equal(expectedRecords.Count, actualRecords.Count);
        Assert.Equal(
            JsonSerializer.Serialize(expectedRecords),
            JsonSerializer.Serialize(actualRecords)
        );
    }

    [Fact]
    public void AddGameRecordTest()
    {
        var request = new RecordedGameRequest { GameName = "pong", Values = [new RecordedGameValue()] };
        _gameRecordService.AddGameRecord(request, "email@prz.edu.pl");

        _contextMock.Verify(c => c.RecordedGames.Add(It.IsAny<RecordedGame>()), Times.Once);
    }
}