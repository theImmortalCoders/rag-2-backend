#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using Xunit;

#endregion

namespace rag_2_backend.Test.Dao;

public class GameDaoTests
{
    private readonly Mock<DatabaseContext> _dbContextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly GameDao _gameDao;

    public GameDaoTests()
    {
        _gameDao = new GameDao(_dbContextMock.Object);
    }

    private void SetUpGameDbSet(IEnumerable<Game> games)
    {
        _dbContextMock.Setup(db => db.Games).ReturnsDbSet(games);
    }

    [Fact]
    public async Task GetGameByIdOrThrow_ShouldReturnGame_WhenGameExists()
    {
        const int gameId = 1;
        var expectedGame = new Game { Id = gameId, Name = "Test Game" };
        SetUpGameDbSet(new List<Game> { expectedGame });

        var result = await _gameDao.GetGameByIdOrThrow(gameId);

        Assert.Equal(expectedGame, result);
    }

    [Fact]
    public async Task GetGameByIdOrThrow_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        const int gameId = 1;
        SetUpGameDbSet(new List<Game>());

        await Assert.ThrowsAsync<NotFoundException>(() => _gameDao.GetGameByIdOrThrow(gameId));
    }

    [Fact]
    public async Task GetGameByNameOrThrow_ShouldReturnGame_WhenGameWithMatchingNameExists()
    {
        const string gameName = "Test Game";
        var expectedGame = new Game { Id = 1, Name = gameName };
        SetUpGameDbSet(new List<Game> { expectedGame });

        var result = await _gameDao.GetGameByNameOrThrow(gameName);

        Assert.Equal(expectedGame, result);
    }

    [Fact]
    public async Task GetGameByNameOrThrow_ShouldThrowNotFoundException_WhenGameWithMatchingNameDoesNotExist()
    {
        SetUpGameDbSet(new List<Game>());

        await Assert.ThrowsAsync<NotFoundException>(() => _gameDao.GetGameByNameOrThrow("g"));
    }
}