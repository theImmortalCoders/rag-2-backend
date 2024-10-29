#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Administration;

[ApiController]
[Route("api/[controller]")]
public class AdministrationController(AdministrationService administrationService) : ControllerBase
{
    /// <summary>Change ban status for any user by user ID despite admins (Admin)</summary>
    /// <response code="404">User not found</response>
    /// <response code="400">Cannot ban administrator</response>
    [HttpPost("{userId:int}/ban-status")]
    [Authorize(Roles = "Admin")]
    public void ChangeBanStatus([Required] int userId, [Required] bool isBanned)
    {
        administrationService.ChangeBanStatus(userId, isBanned);
    }

    /// <summary>Change role for any user by user ID despite admins (Admin)</summary>
    /// <response code="404">User not found</response>
    /// <response code="400">Cannot change administrator's role</response>
    [HttpPost("{userId:int}/role")]
    [Authorize(Roles = "Admin")]
    public void ChangeRole([Required] int userId, [Required] Role role)
    {
        administrationService.ChangeRole(userId, role);
    }

    /// <summary>Get details of any user by user ID, only yours if not admin or teacher (Auth)</summary>
    /// <response code="403">Cannot view details</response>
    [HttpGet("{userId:int}/details")]
    [Authorize]
    public UserResponse GetUserDetails([Required] int userId)
    {
        return administrationService.GetUserDetails(UserDao.GetPrincipalEmail(User), userId);
    }

    /// <summary>Get all users list (Admin, Teacher)</summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin, Teacher")]
    public List<UserResponse> GetStudents()
    {
        return administrationService.GetStudents();
    }
}