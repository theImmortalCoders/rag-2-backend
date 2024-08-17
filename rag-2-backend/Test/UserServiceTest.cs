using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.Models;
using rag_2_backend.models.entity;
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

    private readonly Mock<JwtUtil> _jwtUtilMock = new(null, null);
    private readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock = new();
    private readonly Mock<EmailService> _emailService = new(null, null);
    private readonly UserService _userService;

    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Password = HashUtil.HashPassword("password"),
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023
    };

    private readonly AccountConfirmationToken _accountToken;
    private readonly PasswordResetToken _passwordToken;

    public UserServiceTest()
    {
        _accountToken = new AccountConfirmationToken
        {
            User = _user,
            Expiration = DateTime.Now.AddDays(7),
            Token = HashUtil.HashPassword("password"),
        };
        _passwordToken = new PasswordResetToken()
        {
            User = _user,
            Expiration = DateTime.Now.AddDays(7),
            Token = HashUtil.HashPassword("password"),
        };
        _userService = new UserService(_contextMock.Object, _jwtUtilMock.Object, _emailService.Object,
            _jwtSecurityTokenHandlerMock.Object);

        _contextMock.Setup(c => c.Users).Returns(() => new List<User> { _user }
            .AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.AccountConfirmationTokens)
            .Returns(() => new List<AccountConfirmationToken> { _accountToken }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.BlacklistedJwts)
            .Returns(() => new List<BlacklistedJwt>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.RecordedGames)
            .Returns(() => new List<RecordedGame>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.PasswordResetTokens)
            .Returns(() => new List<PasswordResetToken>() { _passwordToken }.AsQueryable().BuildMockDbSet().Object);
        _jwtUtilMock.Setup(j => j.GenerateToken(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _emailService.Setup(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _emailService.Setup(e => e.SendPasswordResetMail(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _jwtSecurityTokenHandlerMock.Setup(e => e.ReadToken(It.IsAny<string>())).Returns(() => new JwtSecurityToken());
    }

    [Fact]
    public void ShouldRegisterUser()
    {
        _userService.RegisterUser(new UserRequest
            { Email = "email1@prz.edu.pl", Password = "pass", StudyCycleYearA = 2022, StudyCycleYearB = 2023 }
        );

        _contextMock.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once);
        _contextMock.Verify(c => c.AccountConfirmationTokens.Add(It.IsAny<AccountConfirmationToken>()), Times.Once);
        _emailService.Verify(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Assert.Throws<BadHttpRequestException>(
            () => _userService.RegisterUser(new UserRequest
                { Email = "email1@stud.prz.edu.pl", Password = "pass", StudyCycleYearA = 2020, StudyCycleYearB = 2023 }
            )
        );
    }

    [Fact]
    public void ShouldResendConfirmationMail()
    {
        _userService.ResendConfirmationEmail("email@prz.edu.pl");

        _contextMock.Verify(c => c.AccountConfirmationTokens.Add(It.IsAny<AccountConfirmationToken>()), Times.Once);
        _emailService.Verify(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _user.Confirmed = true;
        Assert.Throws<BadHttpRequestException>(() => _userService.ResendConfirmationEmail("email@prz.edu.pl"));
    }

    [Fact]
    public void ShouldConfirmAccount()
    {
        _userService.ConfirmAccount(_accountToken.Token);
        _contextMock.Verify(c => c.AccountConfirmationTokens.Remove(It.IsAny<AccountConfirmationToken>()), Times.Once);

        Assert.Throws<BadHttpRequestException>(() => _userService.ConfirmAccount("token")); //wrong token
        _accountToken.Expiration = DateTime.Now.AddDays(-7);
        Assert.Throws<BadHttpRequestException>(() => _userService.ConfirmAccount(_accountToken.Token)); //invalid date
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
    public void ShouldLogoutUser()
    {
        _userService.LogoutUser("Bearer header");

        _contextMock.Verify(c => c.BlacklistedJwts.Add(It.IsAny<BlacklistedJwt>()), Times.Once);
    }

    [Fact]
    public void ShouldGetMe()
    {
        var userResponse = new UserResponse()
        {
            Id = 1,
            Email = "email@prz.edu.pl",
            Role = Role.Teacher,
            StudyCycleYearA = 2022,
            StudyCycleYearB = 2023
        };

        Assert.Equal(JsonConvert.SerializeObject(userResponse),
            JsonConvert.SerializeObject(_userService.GetMe("email@prz.edu.pl").Result));
    }

    [Fact]
    public void ShouldRequestPasswordReset()
    {
        _userService.RequestPasswordReset("email@prz.edu.pl");

        _contextMock.Verify(c => c.PasswordResetTokens.Add(It.IsAny<PasswordResetToken>()), Times.Once);
        _emailService.Verify(e => e.SendPasswordResetMail(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ShouldResetPassword()
    {
        _userService.ResetPassword(_passwordToken.Token, "pass");
        _contextMock.Verify(c => c.PasswordResetTokens.Remove(It.IsAny<PasswordResetToken>()), Times.Once);

        Assert.Throws<BadHttpRequestException>(() => _userService.ResetPassword("token1", "pass")); //wrong token
        _passwordToken.Expiration = DateTime.Now.AddDays(-7);
        Assert.Throws<BadHttpRequestException>(() =>
            _userService.ResetPassword(_passwordToken.Token, "pass")); //invalid date
    }

    [Fact]
    public void ShouldChangePassword()
    {
        _userService.ChangePassword("email@prz.edu.pl", "password", "pass2");

        Assert.Throws<BadHttpRequestException>(() => _userService.ChangePassword("email@prz.edu.pl", "pa4ss2", "pas2"));
    }

    [Fact]
    public void ShouldDeleteAccount()
    {
        _userService.DeleteAccount("email@prz.edu.pl", "Bearer header");

        _contextMock.Verify(c => c.Users.Remove(It.IsAny<User>()), Times.Once);
    }
}