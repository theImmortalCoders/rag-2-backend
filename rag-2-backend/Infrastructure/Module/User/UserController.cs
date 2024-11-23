#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.User;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    /// <summary>Register new user</summary>
    /// <response code="400">User already exists or wrong data</response>
    [HttpPost("register")]
    public async Task Register([FromBody] [Required] UserRequest userRequest)
    {
        await userService.RegisterUser(userRequest);
    }

    /// <summary>Resend confirmation email to specified email</summary>
    /// <response code="404">User not found</response>
    /// <response code="400">User is already confirmed</response>
    [HttpPost("resend-confirmation-email")]
    public async Task ResendConfirmationEmail([Required] string email)
    {
        await userService.ResendConfirmationEmail(email);
    }

    /// <summary>Confirm account with token from mail</summary>
    /// <response code="400">Invalid token</response>
    [HttpPost("confirm-account")]
    public async Task ConfirmAccount([Required] string token)
    {
        await userService.ConfirmAccount(token);
    }

    /// <summary>Edit account info (Auth)</summary>
    /// <response code="400">Wrong data</response>
    [HttpPatch("update")]
    [Authorize]
    public async Task UpdateAccount([Required] UserEditRequest request)
    {
        await userService.UpdateAccount(request, AuthDao.GetPrincipalEmail(User));
    }

    /// <summary>Request password reset for given email</summary>
    [HttpPost("request-password-reset")]
    public async Task RequestPasswordReset([Required] string email)
    {
        await userService.RequestPasswordReset(email);
    }

    /// <summary>Reset password with token and new password</summary>
    /// <response code="400">Invalid token</response>
    [HttpPost("reset-password")]
    public async Task ResetPassword([Required] string tokenValue, [Required] string newPassword)
    {
        await userService.ResetPassword(tokenValue, newPassword);
    }

    /// <summary>Change current user's password (Auth)</summary>
    /// <response code="400">Invalid old password or given the same password as old</response>
    [HttpPost("change-password")]
    [Authorize]
    public async Task ChangePassword([Required] string oldPassword, [Required] string newPassword)
    {
        await userService.ChangePassword(AuthDao.GetPrincipalEmail(User), oldPassword, newPassword);
    }

    /// <summary>Permanently delete account and all data (Auth)</summary>
    [HttpDelete("delete-account")]
    [Authorize]
    public async Task DeleteAccount()
    {
        var header = HttpContext.Request.Headers.Authorization.FirstOrDefault() ??
                     throw new UnauthorizedAccessException("Unauthorized");

        await userService.DeleteAccount(AuthDao.GetPrincipalEmail(User), header);
    }
}