#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Game;
using rag_2_backend.Infrastructure.Module.Game.Dto;
using Xunit;

#endregion

namespace rag_2_backend.Test.Service;

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
        Mock<GameDao> gameDaoMock = new(_contextMock.Object);
        _gameService = new GameService(_contextMock.Object, gameDaoMock.Object);
        gameDaoMock.Setup(dao => dao.GetGameByIdOrThrow(It.IsAny<int>())).Returns(_games[0]);
        _contextMock.Setup(c => c.Games).Returns(_games.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.GameRecords)
            .Returns(new List<GameRecord>().AsQueryable().BuildMockDbSet().Object);
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

        Assert.Throws<BadRequestException>(() => _gameService.AddGame(gameRequest));
    }

    [Fact]
    public void ShouldRemoveGame()
    {
        _gameService.RemoveGame(1);

        _contextMock.Verify(c => c.Games.Remove(It.Is<Game>(g => g.Id == 1)), Times.Once);
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

        Assert.Throws<BadRequestException>(() => _gameService.EditGame(gameRequest, 1));
    }
}