using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.models.entity;
using rag_2_backend.Models;
using rag_2_backend.Services;
using Newtonsoft.Json;
using Xunit;

namespace rag_2_backend.Test;

public class GameServiceTest
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );
    private readonly GameService _gameService;
    private readonly List<Game> _games =
    [
        new() { Id = 1, Name = "Game1", GameType = GameType.EventGame },
        new() { Id = 2, Name = "Game2", GameType = GameType.TimeGame },
    ];

    public GameServiceTest()
    {
        _gameService = new GameService(_contextMock.Object);
        _contextMock.Setup(c => c.Games).Returns(_games.AsQueryable().BuildMockDbSet().Object);
    }

    [Fact]
    public void ShouldGetAllGames()
    {
        var actualGames = _gameService.GetGames().Result;

        Assert.Equal(JsonConvert.SerializeObject(_games.Select(GameMapper.Map)), JsonConvert.SerializeObject(actualGames));
    }

    [Fact]
    public void ShouldAddGame()
    {
        var gameRequest = new GameRequest
        {
            Name = "Game3",
            GameType = GameType.EventGame
        };

        _gameService.AddGame(gameRequest);

        _contextMock.Verify(c => c.Games.Add(It.Is<Game>(g => g.Name == gameRequest.Name && g.GameType == gameRequest.GameType)), Times.Once);
    }

    [Fact]
    public void ShouldNotAddGameIfGameAlreadyExists()
    {
        var gameRequest = new GameRequest
        {
            Name = "Game1",
            GameType = GameType.EventGame
        };

        Assert.Throws<BadHttpRequestException>(() => _gameService.AddGame(gameRequest));
    }
}