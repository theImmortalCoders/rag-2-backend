using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.Models.Entity;
using rag_2_backend.Services;
using rag_2_backend.Utils;
using Xunit;

namespace rag_2_backend.Test;

public class UserServiceTest
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );
    private readonly Mock<JwtUtil> _jwtUtilMock = new(null);
    private readonly Mock<EmailService> _emailService = new(null);
    private readonly UserService _userService;
    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Password = HashUtil.HashPassword("password"),
    };

    public UserServiceTest()
    {
        _userService = new UserService(_contextMock.Object, _jwtUtilMock.Object, _emailService.Object);

        _contextMock.Setup(c => c.Users).Returns(() => new List<User> { _user }
            .AsQueryable().BuildMockDbSet().Object);
        _jwtUtilMock.Setup(j=>j.GenerateToken(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
    }

    [Fact]
    public void ShouldRegisterUser()
    {
        _userService.RegisterUser(new UserRequest
            {Email = "email1@prz.edu.pl", Password = "pass"}
        );

        _contextMock.Verify(c=>c.Users.Add(It.IsAny<User>()), Times.Once);
    }

    // [Fact]
    // public void ShouldLoginUser()
    // {
    //     Assert.ThrowsAsync<UnauthorizedAccessException>(
    //         () => _userService.LoginUser("email@prz.edu.pl", "pass")
    //     ); //wrong password
    //
    //     Assert.ThrowsAsync<UnauthorizedAccessException>(
    //         () => _userService.LoginUser("email@prz.edu.pl", "password")
    //     ); //not confirmed
    //
    //     _user.Confirmed = true;
    //     _userService.LoginUser("email@prz.edu.pl", "password");
    //     _jwtUtilMock.Verify(j=>j.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    // }

}