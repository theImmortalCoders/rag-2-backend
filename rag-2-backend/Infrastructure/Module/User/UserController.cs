#region

using System.ComponentModel.DataAnnotations;
using HttpExceptions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.User;

[ApiController]
[Route("api/[controller]/auth")]
public class UserController(UserService userService, IConfiguration config) : ControllerBase
{
    /// <summary>Register new user</summary>
    /// <response code="400">User already exists or wrong study cycle year</response>
    [HttpPost("register")]
    public void Register([FromBody] [Required] UserRequest userRequest)
    {
        userService.RegisterUser(userRequest);
    }

    /// <summary>Authenticate</summary>
    /// <response code="401">Invalid password or mail not confirmed or user banned</response>
    [HttpPost("login")]
    public string Login([FromBody] [Required] UserLoginRequest loginRequest)
    {
        var response = userService.LoginUser(
            loginRequest.Email,
            loginRequest.Password,
            double.Parse(config["RefreshToken:ExpireDays"] ?? "30")
        );

        HttpContext.Response.Cookies.Append("refreshToken", response.RefreshToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(
                    double.Parse(config["RefreshToken:ExpireDays"] ?? "30")),
                HttpOnly = true
                // IsEssential = true,
                // Secure = true,
                // SameSite = SameSiteMode.None
            });
        return response.JwtToken;
    }

    /// <summary>Resend confirmation email to specified email</summary>
    /// <response code="404">User not found</response>
    /// <response code="400">User is already confirmed</response>
    [HttpPost("resend-confirmation-email")]
    public void ResendConfirmationEmail([Required] string email)
    {
        userService.ResendConfirmationEmail(email);
    }

    /// <summary>Confirm account with token from mail</summary>
    /// <response code="400">Invalid token</response>
    [HttpPost("confirm-account")]
    public void ConfirmAccount([Required] string token)
    {
        userService.ConfirmAccount(token);
    }

    /// <summary>Request password reset for given email</summary>
    [HttpPost("request-password-reset")]
    public void RequestPasswordReset([Required] string email)
    {
        userService.RequestPasswordReset(email);
    }

    /// <summary>Reset password with token and new password</summary>
    /// <response code="400">Invalid token</response>
    [HttpPost("reset-password")]
    public void ResetPassword([Required] string tokenValue, [Required] string newPassword)
    {
        userService.ResetPassword(tokenValue, newPassword);
    }

    /// <summary>Refresh token</summary>
    /// <response code="401">Invalid refresh token</response>
    [HttpPost("refresh-token")]
    public string RefreshToken()
    {
        HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
        if (refreshToken == null)
            throw new UnauthorizedException("Invalid refresh token");

        return userService.RefreshToken(refreshToken);
    }

    /// <summary>Logout current user (Auth)</summary>
    [HttpPost("logout")]
    [Authorize]
    public void Logout()
    {
        userService.LogoutUser(UserDao.GetPrincipalEmail(User));
    }

    /// <summary>Get current user details (Auth)</summary>
    [HttpGet("me")]
    [Authorize]
    public UserResponse Me()
    {
        return userService.GetMe(UserDao.GetPrincipalEmail(User));
    }

    /// <summary>Change current user's password (Auth)</summary>
    /// <response code="400">Invalid old password or given the same password as old</response>
    [HttpPost("change-password")]
    [Authorize]
    public void ChangePassword([Required] string oldPassword, [Required] string newPassword)
    {
        userService.ChangePassword(UserDao.GetPrincipalEmail(User), oldPassword, newPassword);
    }

    /// <summary>Permanently delete account and all data (Auth)</summary>
    [HttpDelete("delete-account")]
    [Authorize]
    public void DeleteAccount()
    {
        var header = HttpContext.Request.Headers.Authorization.FirstOrDefault() ??
                     throw new UnauthorizedAccessException("Unauthorized");

        userService.DeleteAccount(UserDao.GetPrincipalEmail(User), header);
    }
}