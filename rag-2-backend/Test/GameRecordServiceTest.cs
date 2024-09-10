using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.Models;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;
using rag_2_backend.Services;
using Xunit;

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

        _gameRecordService = new GameRecordService(_contextMock.Object);

        _recordedGames.Add(new RecordedGame
        {
            Id = 1,
            Game = _game,
            Value = "10",
            User = _user
        });
    }

    [Fact]
    public async void GetRecordsByGameTest()
    {
        var actualRecords = _gameRecordService.GetRecordsByGame(1);
        List<RecordedGameResponse> expectedRecords =
        [
            new RecordedGameResponse
            {
                Id = 1,
                Value = "10",
                GameResponse = new GameResponse { Id = 1, Name = "pong" },
                UserResponse = new UserResponse
                {
                    Id = 1, Email = "email@prz.edu.pl", Role = Role.Teacher, StudyCycleYearA = 2022,
                    Name = "John",
                    StudyCycleYearB = 2023
                }
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
        var request = new RecordedGameRequest { GameName = "pong", Value = "10" };
        _gameRecordService.AddGameRecord(request, "email@prz.edu.pl");

        _contextMock.Verify(c => c.RecordedGames.Add(It.IsAny<RecordedGame>()), Times.Once);
    }
}