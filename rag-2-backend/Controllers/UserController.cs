#region

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO.User;
using rag_2_backend.Services;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]/auth")]
public class UserController(UserService userService) : ControllerBase
{
    /// <response code="400">User already exists or wrong study cycle year</response>
    [HttpPost("register")]
    public void Register([FromBody] [Required] UserRequest userRequest)
    {
        userService.RegisterUser(userRequest);
    }

    /// <response code="401">Invalid password or mail not confirmed or user banned</response>
    [HttpPost("login")]
    public string Login([FromBody] [Required] UserLoginRequest loginRequest)
    {
        return userService.LoginUser(loginRequest.Email, loginRequest.Password);
    }

    /// <response code="404">User not found</response>
    /// <response code="400">User is already confirmed</response>
    [HttpPost("resend-confirmation-email")]
    public void ResendConfirmationEmail([Required] string email)
    {
        userService.ResendConfirmationEmail(email);
    }

    /// <response code="400">Invalid token</response>
    [HttpPost("confirm-account")]
    public void ConfirmAccount([Required] string token)
    {
        userService.ConfirmAccount(token);
    }

    [HttpPost("request-password-reset")]
    public void RequestPasswordReset([Required] string email)
    {
        userService.RequestPasswordReset(email);
    }

    /// <response code="400">Invalid token</response>
    [HttpPost("reset-password")]
    public void ResetPassword([Required] string tokenValue, [Required] string newPassword)
    {
        userService.ResetPassword(tokenValue, newPassword);
    }

    /// <summary>(Auth)</summary>
    [HttpPost("logout")]
    [Authorize]
    public void Logout()
    {
        var header = HttpContext.Request.Headers.Authorization.FirstOrDefault() ??
                     throw new UnauthorizedAccessException("Unauthorized");

        userService.LogoutUser(header);
    }

    /// <summary>(Auth)</summary>
    [HttpGet("me")]
    [Authorize]
    public UserResponse Me()
    {
        return userService.GetMe(UserUtil.GetPrincipalEmail(User));
    }

    /// <summary>(Auth)</summary>
    /// <response code="400">Invalid old password or given the same password as old</response>
    [HttpPost("change-password")]
    [Authorize]
    public void ChangePassword([Required] string oldPassword, [Required] string newPassword)
    {
        userService.ChangePassword(UserUtil.GetPrincipalEmail(User), oldPassword, newPassword);
    }

    /// <summary> (Auth)</summary>
    [HttpDelete("delete-account")]
    [Authorize]
    public void DeleteAccount()
    {
        var header = HttpContext.Request.Headers.Authorization.FirstOrDefault() ??
                     throw new UnauthorizedAccessException("Unauthorized");

        userService.DeleteAccount(UserUtil.GetPrincipalEmail(User), header);
    }
}