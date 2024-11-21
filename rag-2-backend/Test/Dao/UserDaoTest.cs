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

public class UserDaoTests
{
    private readonly Mock<DatabaseContext> _dbContextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly UserDao _userDao;

    public UserDaoTests()
    {
        _userDao = new UserDao(_dbContextMock.Object);
    }

    private void SetUpUsersDbSet(IEnumerable<User> users)
    {
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(users);
    }

    [Fact]
    public async Task GetUserByIdOrThrow_ShouldReturnUser_WhenUserExists()
    {
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Password = null!,
            Name = null!
        };
        SetUpUsersDbSet(new List<User> { user });

        var result = await _userDao.GetUserByIdOrThrow(userId);

        Assert.Equal(userId, result.Id);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdOrThrow_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        const int userId = 1;
        SetUpUsersDbSet(new List<User>());

        await Assert.ThrowsAsync<NotFoundException>(() => _userDao.GetUserByIdOrThrow(userId));
    }

    [Fact]
    public async Task GetUserByEmailOrThrow_ShouldReturnUser_WhenUserExists()
    {
        const string email = "test@example.com";
        var user = new User
        {
            Id = 1,
            Email = email,
            Password = null!,
            Name = null!
        };
        SetUpUsersDbSet(new List<User> { user });

        var result = await _userDao.GetUserByEmailOrThrow(email);

        Assert.Equal(email, result.Email);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetUserByEmailOrThrow_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        const string email = "test@example.com";
        SetUpUsersDbSet(new List<User>());

        await Assert.ThrowsAsync<NotFoundException>(() => _userDao.GetUserByEmailOrThrow(email));
    }
}