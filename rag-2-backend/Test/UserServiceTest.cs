#region

using System.IdentityModel.Tokens.Jwt;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Email;
using rag_2_backend.Infrastructure.Module.User;
using rag_2_backend.Infrastructure.Module.User.Dto;
using rag_2_backend.Infrastructure.Util;
using Xunit;

#endregion

namespace rag_2_backend.Test;

public class UserServiceTest
{
    private readonly AccountConfirmationToken _accountToken;

    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly Mock<EmailService> _emailService = new(null!, null!);
    private readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock = new();

    private readonly Mock<JwtUtil> _jwtUtilMock = new(null!);
    private readonly PasswordResetToken _passwordToken;

    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Name = "John",
        Password = HashUtil.HashPassword("password"),
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023
    };

    private readonly UserService _userService;

    public UserServiceTest()
    {
        _accountToken = new AccountConfirmationToken
        {
            User = _user,
            Expiration = DateTime.Now.AddDays(7),
            Token = HashUtil.HashPassword("password")
        };
        _passwordToken = new PasswordResetToken
        {
            User = _user,
            Expiration = DateTime.Now.AddDays(7),
            Token = HashUtil.HashPassword("password")
        };

        Mock<UserDao> userMock = new(_contextMock.Object);
        Mock<RefreshTokenDao> refreshTokenDaoMock = new(_contextMock.Object);
        userMock.Setup(u => u.GetUserByIdOrThrow(It.IsAny<int>())).Returns(_user);
        userMock.Setup(u => u.GetUserByEmailOrThrow(It.IsAny<string>())).Returns(_user);

        _userService = new UserService(_contextMock.Object, _jwtUtilMock.Object, _emailService.Object,
            userMock.Object, refreshTokenDaoMock.Object);

        _contextMock.Setup(c => c.Users).Returns(() => new List<User> { _user }
            .AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.AccountConfirmationTokens)
            .Returns(() => new List<AccountConfirmationToken> { _accountToken }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.RefreshTokens)
            .Returns(() => new List<RefreshToken>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.RecordedGames)
            .Returns(() => new List<GameRecord>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.PasswordResetTokens)
            .Returns(() => new List<PasswordResetToken> { _passwordToken }.AsQueryable().BuildMockDbSet().Object);
        _jwtUtilMock.Setup(j => j.GenerateJwt(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _emailService.Setup(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _emailService.Setup(e => e.SendPasswordResetMail(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _jwtSecurityTokenHandlerMock.Setup(e => e.ReadToken(It.IsAny<string>())).Returns(() => new JwtSecurityToken());
    }

    [Fact]
    public void ShouldRegisterUser()
    {
        _userService.RegisterUser(new UserRequest
            {
                Email = "email1@prz.edu.pl", Password = "pass", StudyCycleYearA = 2022, StudyCycleYearB = 2023,
                Name = "John"
            }
        );

        _contextMock.Verify(c => c.Users.Add(It.IsAny<User>()), Times.Once);
        _contextMock.Verify(c => c.AccountConfirmationTokens.Add(It.IsAny<AccountConfirmationToken>()), Times.Once);
        _emailService.Verify(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Assert.Throws<BadRequestException>(
            () => _userService.RegisterUser(new UserRequest
                {
                    Email = "email1@stud.prz.edu.pl", Password = "pass", StudyCycleYearA = 2020, StudyCycleYearB = 2023,
                    Name = "John"
                }
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
        Assert.Throws<BadRequestException>(() => _userService.ResendConfirmationEmail("email@prz.edu.pl"));
    }

    [Fact]
    public void ShouldConfirmAccount()
    {
        _userService.ConfirmAccount(_accountToken.Token);
        _contextMock.Verify(c => c.AccountConfirmationTokens.Remove(It.IsAny<AccountConfirmationToken>()), Times.Once);

        Assert.Throws<BadRequestException>(() => _userService.ConfirmAccount("token")); //wrong token
        _accountToken.Expiration = DateTime.Now.AddDays(-7);
        Assert.Throws<BadRequestException>(() => _userService.ConfirmAccount(_accountToken.Token)); //invalid date
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

        Assert.Throws<BadRequestException>(() => _userService.ResetPassword("token1", "pass")); //wrong token
        _passwordToken.Expiration = DateTime.Now.AddDays(-7);
        Assert.Throws<BadRequestException>(() =>
            _userService.ResetPassword(_passwordToken.Token, "pass")); //invalid date
    }

    [Fact]
    public void ShouldChangePassword()
    {
        _userService.ChangePassword("email@prz.edu.pl", "password", "pass2");

        Assert.Throws<BadRequestException>(() => _userService.ChangePassword("email@prz.edu.pl", "pa4ss2", "pas2"));
    }

    [Fact]
    public void ShouldDeleteAccount()
    {
        _userService.DeleteAccount("email@prz.edu.pl", "Bearer header");

        _contextMock.Verify(c => c.Users.Remove(It.IsAny<User>()), Times.Once);
    }
}