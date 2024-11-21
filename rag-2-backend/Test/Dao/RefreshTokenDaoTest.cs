#region

using Microsoft.EntityFrameworkCore;
using Moq;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using Xunit;

#endregion

namespace rag_2_backend.Test.Dao;

public class RefreshTokenDaoTests
{
    private readonly Mock<DatabaseContext> _dbContextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly RefreshTokenDao _refreshTokenDao;

    public RefreshTokenDaoTests()
    {
        _refreshTokenDao = new RefreshTokenDao(_dbContextMock.Object);
    }

    private void SetUpRefreshTokensDbSet(IEnumerable<RefreshToken> tokens)
    {
        var tokensQueryable = tokens.AsQueryable();
        var tokensDbSetMock = new Mock<DbSet<RefreshToken>>();
        tokensDbSetMock.As<IQueryable<RefreshToken>>().Setup(m => m.Provider).Returns(tokensQueryable.Provider);
        tokensDbSetMock.As<IQueryable<RefreshToken>>().Setup(m => m.Expression).Returns(tokensQueryable.Expression);
        tokensDbSetMock.As<IQueryable<RefreshToken>>().Setup(m => m.ElementType).Returns(tokensQueryable.ElementType);
        using var enumerator = tokensQueryable.GetEnumerator();
        tokensDbSetMock.As<IQueryable<RefreshToken>>().Setup(m => m.GetEnumerator()).Returns(enumerator);

        _dbContextMock.Setup(db => db.RefreshTokens).Returns(tokensDbSetMock.Object);
    }

    [Fact]
    public async Task RemoveTokensForUser_ShouldRemoveAllTokensForSpecifiedUser()
    {
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            Password = null!,
            Name = null!
        };
        var tokens = new List<RefreshToken>
        {
            new()
            {
                User = user,
                Token = null!,
                Expiration = default
            },
            new()
            {
                User = user,
                Token = null!,
                Expiration = default
            }
        };

        SetUpRefreshTokensDbSet(tokens);

        await _refreshTokenDao.RemoveTokensForUser(user);

        _dbContextMock.Verify(db => db.RefreshTokens
            .RemoveRange(It.Is<IEnumerable<RefreshToken>>
                (r => r.All(t => t.User.Id == userId))), Times.Once);
        _dbContextMock.Verify(db => db.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task RemoveTokensForUser_ShouldNotThrow_WhenNoTokensExistForUser()
    {
        var user = new User
        {
            Id = 1,
            Password = null!,
            Name = null!
        };
        SetUpRefreshTokensDbSet(new List<RefreshToken>());

        var exception = await Record.ExceptionAsync(() => _refreshTokenDao.RemoveTokensForUser(user));

        Assert.Null(exception);
        _dbContextMock.Verify(db => db.RefreshTokens
            .RemoveRange(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChanges(), Times.Once);
    }
}