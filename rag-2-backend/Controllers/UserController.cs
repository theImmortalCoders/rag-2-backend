using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO;
using rag_2_backend.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    [HttpPost("auth/register")]
    public void Register([FromBody] [Required] UserRequest userRequest)
    {
        userService.RegisterUser(userRequest);
    }

    [HttpPost("auth/login")]
    public async Task<string> Login([FromBody] [Required] UserLoginRequest loginRequest)
    {
        return await userService.LoginUser(loginRequest.Email, loginRequest.Password);
    }

    [HttpPost("auth/logout")]
    [Authorize]
    public void Logout()
    {
        var header = HttpContext.Request.Headers.Authorization.FirstOrDefault() ??
                     throw new UnauthorizedAccessException("Unauthorized");

        userService.LogoutUser(header);
    }

    [HttpPost("auth/resend-confirmation-email")]
    public void ResendConfirmationEmail([Required] string email)
    {
        userService.ResendConfirmationEmail(email);
    }

    [HttpPost("auth/confirm-account")]
    public void ConfirmAccount([Required] string token)
    {
        userService.ConfirmAccount(token);
    }

    /// <summary>
    /// (Auth)
    /// </summary>
    [HttpGet("auth/me")]
    [Authorize]
    public async Task<UserResponse> Me()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new UnauthorizedAccessException("Unauthorized");

        return await userService.GetMe(email);
    }

    [HttpPost("auth/request-password-reset")]
    public void RequestPasswordReset([Required] string email)
    {
        userService.RequestPasswordReset(email);
    }

    [HttpPost("auth/reset-password")]
    public void ResetPassword([Required] string tokenValue, [Required] string newPassword)
    {
        userService.ResetPassword(tokenValue, newPassword);
    }
}