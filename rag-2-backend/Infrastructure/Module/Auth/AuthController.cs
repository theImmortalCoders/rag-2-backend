#region

using System.ComponentModel.DataAnnotations;
using HttpExceptions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Dao;
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
    public void VerifyToken()
    {
    }

    /// <summary>Authenticate</summary>
    /// <response code="401">Invalid password or mail not confirmed or user banned</response>
    [HttpPost("login")]
    public string Login([FromBody] [Required] UserLoginRequest loginRequest)
    {
        var response = authService.LoginUser(
            loginRequest.Email,
            loginRequest.Password,
            double.Parse(config["RefreshToken:ExpireDays"] ?? "30")
        );

        HttpContext.Response.Cookies.Append("refreshToken", response.RefreshToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(
                    double.Parse(config["RefreshToken:ExpireDays"] ?? "30")),
                HttpOnly = true,
                // IsEssential = true,
                Secure = false
                // SameSite = SameSiteMode.None
            });
        return response.JwtToken;
    }

    /// <summary>Refresh token</summary>
    /// <response code="401">Invalid refresh token</response>
    [HttpPost("refresh-token")]
    public string RefreshToken()
    {
        HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
        if (refreshToken == null)
            throw new UnauthorizedException("Invalid refresh token");

        return authService.RefreshToken(refreshToken);
    }

    /// <summary>Logout current user (Auth)</summary>
    [HttpPost("logout")]
    [Authorize]
    public void Logout()
    {
        authService.LogoutUser(AuthDao.GetPrincipalEmail(User));
    }

    /// <summary>Get current user details (Auth)</summary>
    [HttpGet("me")]
    [Authorize]
    public UserResponse Me()
    {
        return authService.GetMe(AuthDao.GetPrincipalEmail(User));
    }
}