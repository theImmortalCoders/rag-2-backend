#region

using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.Config;
using rag_2_backend.Models;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;
using rag_2_backend.Services;
using rag_2_backend.Utils;
using Xunit;

#endregion

namespace rag_2_backend.Test;

public class StatsServiceTests
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly Game _game = new()
    {
        Id = 1,
        Name = "pong"
    };

    private readonly Mock<UserUtil> _mockUserUtil;

    private readonly List<RecordedGame> _recordedGames = [];
    private readonly StatsService _statsService;

    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Name = "John",
        Password = HashUtil.HashPassword("password"),
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023
    };

    public StatsServiceTests()
    {
        _mockUserUtil = new Mock<UserUtil>(_contextMock.Object);
        _statsService = new StatsService(_contextMock.Object, _mockUserUtil.Object);

        _contextMock.Setup(c => c.RecordedGames).Returns(
            _recordedGames.AsQueryable().BuildMockDbSet().Object
        );
        _contextMock.Setup(c => c.Users).Returns(
            new List<User> { _user }.AsQueryable().BuildMockDbSet().Object
        );
        _contextMock.Setup(c => c.Games).Returns(
            new List<Game> { _game }.AsQueryable().BuildMockDbSet().Object
        );

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
    public void ShouldGetStatsForUser()
    {
        _mockUserUtil.Setup(u => u.GetUserByEmailOrThrow(It.IsAny<string>())).Returns(_user);

        var result = _statsService.GetStatsForUser("email@prz.edu.pl", 1);

        Assert.Equal(_recordedGames.First().Started, result.FirstPlayed);
        Assert.Equal(_recordedGames.Last().Ended, result.LastPlayed);
        Assert.Equal(1, result.Games);
        Assert.Equal(1, result.Plays);
    }

    [Fact]
    public void ShouldReturnStatsForGame()
    {
        var result = _statsService.GetStatsForGame(1);

        Assert.Equal(_recordedGames.First().Started, result.FirstPlayed);
        Assert.Equal(_recordedGames.Last().Ended, result.LastPlayed);
        Assert.Equal(1, result.Plays);
        Assert.Equal(1, result.TotalPlayers);
    }
}