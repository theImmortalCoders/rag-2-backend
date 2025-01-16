#region

using System.ComponentModel.DataAnnotations;
using HttpExceptions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.Auth.Dto;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration config, AuthService authService) : ControllerBase
{
    /// <summary>Verify JWT token</summary>
    /// <response code="401">Token invalid</response>
    [HttpGet("verify")]
    [Authorize]
    public Task VerifyToken()
    {
        return Task.CompletedTask;
    }

    /// <summary>Authenticate</summary>
    /// <response code="401">Invalid password or mail not confirmed or user banned</response>
    [HttpPost("login")]
    public async Task<string> Login([FromBody] [Required] UserLoginRequest loginRequest)
    {
        var refreshTokenExpiryDays = loginRequest.RememberMe
            ? double.Parse(config["RefreshToken:ExpireDaysRememberMe"] ?? "10")
            : double.Parse(config["RefreshToken:ExpireDays"] ?? "1");

        var response = await authService.LoginUser(
            loginRequest.Email,
            loginRequest.Password,
            refreshTokenExpiryDays
        );

        HttpContext.Response.Cookies.Append("refreshToken", response.RefreshToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(
                    double.Parse(config["RefreshToken:ExpireDays"] ?? "30")),
                HttpOnly = true,
                IsEssential = true,
                Secure = false
                // SameSite = SameSiteMode.None
            });
        return response.JwtToken;
    }

    /// <summary>Refresh token</summary>
    /// <response code="401">Invalid refresh token</response>
    [HttpPost("refresh-token")]
    public async Task<string> RefreshToken()
    {
        HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
        if (refreshToken == null)
            throw new UnauthorizedException("Invalid refresh token");

        return await authService.RefreshToken(refreshToken);
    }

    /// <summary>Logout current user (Auth)</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task Logout()
    {
        HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

        if (refreshToken != null)
            await authService.LogoutUser(refreshToken);
    }

    /// <summary>Get current user details (Auth)</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<UserResponse> Me()
    {
        return await authService.GetMe(AuthDao.GetPrincipalEmail(User));
    }
}