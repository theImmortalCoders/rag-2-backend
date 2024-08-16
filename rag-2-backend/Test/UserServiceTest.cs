using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.Models;
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
    private readonly Mock<EmailService> _emailService = new(null, null);
    private readonly UserService _userService;

    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Password = HashUtil.HashPassword("password"),
    };

    private readonly AccountConfirmationToken _token;

    public UserServiceTest()
    {
        _token = new AccountConfirmationToken
        {
            User = _user,
            Expiration = DateTime.Now.AddDays(7),
            Token = HashUtil.HashPassword("password"),
        };
        _userService = new UserService(_contextMock.Object, _jwtUtilMock.Object, _emailService.Object);

        _contextMock.Setup(c => c.Users).Returns(() => new List<User> { _user }
            .AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.AccountConfirmationTokens)
            .Returns(() => new List<AccountConfirmationToken>() { _token }.AsQueryable().BuildMockDbSet().Object);
        _jwtUtilMock.Setup(j => j.GenerateToken(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _emailService.Setup(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
    }

    [Fact]
    public void ShouldRegisterUser()
    {
        _userService.RegisterUser(new UserRequest
            { Email = "email1@prz.edu.pl", Password = "pass" }
        );

        _contextMock.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once);
        _contextMock.Verify(c => c.AccountConfirmationTokens.Add(It.IsAny<AccountConfirmationToken>()), Times.Once);
        _emailService.Verify(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ShouldResendConfirmationMail()
    {
        _userService.ResendConfirmationEmail("email@prz.edu.pl");

        _contextMock.Verify(c => c.AccountConfirmationTokens.Add(It.IsAny<AccountConfirmationToken>()), Times.Once);
        _emailService.Verify(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _user.Confirmed = true;
        Assert.Throws<BadHttpRequestException>(()=>_userService.ResendConfirmationEmail("email@prz.edu.pl"));
    }

    [Fact]
    public void ShouldConfirmAccount()
    {
        _userService.ConfirmAccount(_token.Token);
        _contextMock.Verify(c => c.AccountConfirmationTokens.Remove(It.IsAny<AccountConfirmationToken>()), Times.Once);

        Assert.Throws<BadHttpRequestException>(() => _userService.ConfirmAccount("token")); //wrong token
        _token.Expiration = DateTime.Now.AddDays(-7);
        Assert.Throws<BadHttpRequestException>(() => _userService.ConfirmAccount(_token.Token)); //invalid date
    }

    [Fact]
    public async void ShouldLoginUser()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.LoginUser("email@prz.edu.pl", "pass")
        ); //wrong password

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _userService.LoginUser("email@prz.edu.pl", "password")
        ); //not confirmed

        _user.Confirmed = true;
        await _userService.LoginUser("email@prz.edu.pl", "password");
        _jwtUtilMock.Verify(j => j.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ShouldGetMe()
    {
        var userResponse = new UserResponse()
        {
            Id = 1,
            Email = "email@prz.edu.pl",
            Role = Role.Teacher,
        };

        Assert.Equal(JsonConvert.SerializeObject(userResponse),
            JsonConvert.SerializeObject(_userService.GetMe("email@prz.edu.pl").Result));
    }
}