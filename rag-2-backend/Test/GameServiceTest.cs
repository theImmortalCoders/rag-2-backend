#region

using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using rag_2_backend.Config;
using rag_2_backend.DTO.Game;
using rag_2_backend.Mapper;
using rag_2_backend.models.entity;
using rag_2_backend.Services;
using Xunit;

#endregion

namespace rag_2_backend.Test;

public class GameServiceTest
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly List<Game> _games =
    [
        new() { Id = 1, Name = "Game1" },
        new() { Id = 2, Name = "Game2" }
    ];

    private readonly GameService _gameService;

    public GameServiceTest()
    {
        _gameService = new GameService(_contextMock.Object);
        _contextMock.Setup(c => c.Games).Returns(_games.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.RecordedGames)
            .Returns(new List<RecordedGame>().AsQueryable().BuildMockDbSet().Object);
    }

    [Fact]
    public void ShouldGetAllGames()
    {
        var actualGames = _gameService.GetGames().Result;

        Assert.Equal(JsonConvert.SerializeObject(_games.Select(GameMapper.Map)),
            JsonConvert.SerializeObject(actualGames));
    }

    [Fact]
    public void ShouldAddGame()
    {
        var gameRequest = new GameRequest
        {
            Name = "Game3"
        };

        _gameService.AddGame(gameRequest);

        _contextMock.Verify(
            c => c.Games.Add(It.Is<Game>(g => g.Name == gameRequest.Name)),
            Times.Once);
    }

    [Fact]
    public void ShouldNotAddGameIfGameAlreadyExists()
    {
        var gameRequest = new GameRequest
        {
            Name = "Game1"
        };

        Assert.Throws<BadHttpRequestException>(() => _gameService.AddGame(gameRequest));
    }

    [Fact]
    public void ShouldRemoveGame()
    {
        _gameService.RemoveGame(1);

        _contextMock.Verify(c => c.Games.Remove(It.Is<Game>(g => g.Id == 1)), Times.Once);
    }

    [Fact]
    public void ShouldNotRemoveGameIfGameNotExists()
    {
        Assert.Throws<KeyNotFoundException>(() => _gameService.RemoveGame(4));
    }

    [Fact]
    public void ShouldUpdateGame()
    {
        var gameRequest = new GameRequest
        {
            Name = "Game3"
        };

        _gameService.EditGame(gameRequest, 1);

        _contextMock.Verify(
            c => c.Games.Update(It.Is<Game>(g => g.Name == gameRequest.Name)),
            Times.Once);
    }

    [Fact]
    public void ShouldNotUpdateGameIfGameAlreadyExists()
    {
        var gameRequest = new GameRequest
        {
            Name = "Game2"
        };

        Assert.Throws<BadHttpRequestException>(() => _gameService.EditGame(gameRequest, 1));
    }

    [Fact]
    public void ShouldThrowNotFoundWhenUpdateGame()
    {
        var gameRequest = new GameRequest
        {
            Name = "Game2"
        };

        Assert.Throws<KeyNotFoundException>(() => _gameService.EditGame(gameRequest, 4));
    }
}