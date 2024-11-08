#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        var gamesQueryable = games.AsQueryable();
        var gamesDbSetMock = new Mock<DbSet<Game>>();
        gamesDbSetMock.As<IQueryable<Game>>().Setup(m => m.Provider).Returns(gamesQueryable.Provider);
        gamesDbSetMock.As<IQueryable<Game>>().Setup(m => m.Expression).Returns(gamesQueryable.Expression);
        gamesDbSetMock.As<IQueryable<Game>>().Setup(m => m.ElementType).Returns(gamesQueryable.ElementType);
        using var enumerator = gamesQueryable.GetEnumerator();
        gamesDbSetMock.As<IQueryable<Game>>().Setup(m => m.GetEnumerator()).Returns(enumerator);

        _dbContextMock.Setup(db => db.Games).Returns(gamesDbSetMock.Object);
    }

    [Fact]
    public void GetGameByIdOrThrow_ShouldReturnGame_WhenGameExists()
    {
        const int gameId = 1;
        var expectedGame = new Game { Id = gameId, Name = "Test Game" };
        SetUpGameDbSet(new List<Game> { expectedGame });

        var result = _gameDao.GetGameByIdOrThrow(gameId);

        Assert.Equal(expectedGame, result);
    }

    [Fact]
    public void GetGameByIdOrThrow_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        const int gameId = 1;
        SetUpGameDbSet(new List<Game>());

        Assert.Throws<NotFoundException>(() => _gameDao.GetGameByIdOrThrow(gameId));
    }

    [Fact]
    public void GetGameByNameOrThrow_ShouldReturnGame_WhenGameWithMatchingNameExists()
    {
        const string gameName = "Test Game";
        var expectedGame = new Game { Id = 1, Name = gameName };
        SetUpGameDbSet(new List<Game> { expectedGame });

        var result = _gameDao.GetGameByNameOrThrow(gameName);

        Assert.Equal(expectedGame, result);
    }

    [Fact]
    public void GetGameByNameOrThrow_ShouldThrowNotFoundException_WhenGameWithMatchingNameDoesNotExist()
    {
        SetUpGameDbSet(new List<Game>());

        Assert.Throws<NotFoundException>(() => _gameDao.GetGameByNameOrThrow("g"));
    }
}