using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO;
using rag_2_backend.Services;

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
    public async Task<string> Login([FromBody] [Required] UserRequest loginRequest)
    {
        return await userService.LoginUser(loginRequest.Email, loginRequest.Password);
    }

    [HttpPost("auth/logout")]
    public void Logout()
    {
        var email = (User.FindFirst(ClaimTypes.Email)?.Value) ?? throw new UnauthorizedAccessException("Unauthorized");

        userService.LogoutUser(email);
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
    /// (Autneticated)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<UserResponse> Me()
    {
        var email = (User.FindFirst(ClaimTypes.Email)?.Value) ?? throw new UnauthorizedAccessException("Unauthorized");

        return await userService.GetMe(email);
    }
}