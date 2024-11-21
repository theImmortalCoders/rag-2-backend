#region

using System.IdentityModel.Tokens.Jwt;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Auth;
using rag_2_backend.Infrastructure.Module.Email;
using rag_2_backend.Infrastructure.Module.User.Dto;
using rag_2_backend.Infrastructure.Util;
using Xunit;

#endregion

namespace rag_2_backend.Test.Service;

public class AuthServiceTest
{
    private readonly AuthService _authService;

    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly Mock<EmailService> _emailService = new(null!, null!);
    private readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock = new();

    private readonly Mock<JwtUtil> _jwtUtilMock = new(null!);

    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Name = "John",
        Password = HashUtil.HashPassword("password"),
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023
    };

    public AuthServiceTest()
    {
        var accountToken = new AccountConfirmationToken
        {
            User = _user,
            Expiration = DateTime.Now.AddDays(7),
            Token = HashUtil.HashPassword("password")
        };
        var passwordToken = new PasswordResetToken
        {
            User = _user,
            Expiration = DateTime.Now.AddDays(7),
            Token = HashUtil.HashPassword("password")
        };

        Mock<UserDao> userMock = new(_contextMock.Object);
        Mock<RefreshTokenDao> refreshTokenDaoMock = new(_contextMock.Object);
        userMock.Setup(u => u.GetUserByIdOrThrow(It.IsAny<int>())).ReturnsAsync(_user);
        userMock.Setup(u => u.GetUserByEmailOrThrow(It.IsAny<string>())).ReturnsAsync(_user);

        _authService = new AuthService(userMock.Object, refreshTokenDaoMock.Object, _contextMock.Object,
            _jwtUtilMock.Object);

        _contextMock.Setup(c => c.Users).Returns(() => new List<User> { _user }
            .AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.AccountConfirmationTokens)
            .Returns(() => new List<AccountConfirmationToken> { accountToken }.AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.RefreshTokens)
            .Returns(() => new List<RefreshToken>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.GameRecords)
            .Returns(() => new List<GameRecord>().AsQueryable().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.PasswordResetTokens)
            .Returns(() => new List<PasswordResetToken> { passwordToken }.AsQueryable().BuildMockDbSet().Object);
        _jwtUtilMock.Setup(j => j.GenerateJwt(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _emailService.Setup(e => e.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _emailService.Setup(e => e.SendPasswordResetMail(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _jwtSecurityTokenHandlerMock.Setup(e => e.ReadToken(It.IsAny<string>())).Returns(() => new JwtSecurityToken());
    }

    [Fact]
    public async Task ShouldLoginUser()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginUser("email@prz.edu.pl", "pass", 30)
        ); //wrong password

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _authService.LoginUser("email@prz.edu.pl", "password", 30)
        ); //not confirmed

        _user.Confirmed = true;
        await _authService.LoginUser("email@prz.edu.pl", "password", 30);
        _jwtUtilMock.Verify(j => j.GenerateJwt(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ShouldLogoutUser()
    {
        await _authService.LogoutUser("Bearer header");
    }

    [Fact]
    public async Task ShouldGetMe()
    {
        var userResponse = new UserResponse
        {
            Id = 1,
            Name = "John",
            Email = "email@prz.edu.pl",
            Role = Role.Teacher,
            StudyCycleYearA = 2022,
            StudyCycleYearB = 2023
        };

        Assert.Equal(JsonConvert.SerializeObject(userResponse),
            JsonConvert.SerializeObject(await _authService.GetMe("email@prz.edu.pl")));
    }
}