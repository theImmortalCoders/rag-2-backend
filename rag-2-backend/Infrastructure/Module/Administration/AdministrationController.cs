#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.Administration.Dto;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Administration;

[ApiController]
[Route("api/[controller]")]
public class AdministrationController(
    AdministrationService administrationService,
    IConfiguration config) : ControllerBase
{
    /// <summary>Change ban status for any user by user ID despite admins (Admin)</summary>
    /// <response code="404">User not found</response>
    /// <response code="400">Cannot ban administrator</response>
    [HttpPost("{userId:int}/ban-status")]
    [Authorize(Roles = "Admin")]
    public async Task ChangeBanStatus([Required] int userId, [Required] bool isBanned)
    {
        await administrationService.ChangeBanStatus(userId, isBanned);
    }

    /// <summary>Change role for any user by user ID despite admins (Admin)</summary>
    /// <response code="404">User not found</response>
    /// <response code="400">Cannot change administrator's role</response>
    [HttpPost("{userId:int}/role")]
    [Authorize(Roles = "Admin")]
    public async Task ChangeRole([Required] int userId, [Required] Role role)
    {
        await administrationService.ChangeRole(userId, role);
    }

    /// <summary>Get details of any user by user ID (Admin, Teacher)</summary>
    /// <response code="403">Cannot view details</response>
    [HttpGet("{userId:int}/details")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<UserResponse> GetUserDetails([Required] int userId)
    {
        return await administrationService.GetUserDetails(AuthDao.GetPrincipalEmail(User), userId);
    }

    /// <summary>Get current limits for roles (Auth)</summary>
    /// <response code="403">Cannot view limits</response>
    [HttpGet("limits")]
    [Authorize]
    public LimitsResponse GetCurrentLimits()
    {
        return new LimitsResponse
        {
            StudentLimitMb = int.Parse(config["StudentDataLimitMb"] ?? "30"),
            TeacherLimitMb = int.Parse(config["TeacherDataLimitMb"] ?? "30"),
            AdminLimitMb = int.Parse(config["AdminDataLimitMb"] ?? "30")
        };
    }

    /// <summary>Get all users list with optional filters and sorting (Admin, Teacher)</summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin, Teacher")]
    public async Task<List<UserResponse>> GetUsers(
        [Required] Role role,
        string? email,
        int? studyCycleYearA,
        int? studyCycleYearB,
        string? group,
        string? courseName,
        SortDirection sortDirection = SortDirection.Asc,
        UserSortByFields sortBy = UserSortByFields.Id
    )
    {
        return await administrationService.GetUsers(
            role,
            email,
            studyCycleYearA,
            studyCycleYearB,
            group,
            courseName,
            sortDirection,
            sortBy
        );
    }
}