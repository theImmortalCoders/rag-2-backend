#region

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO.User;
using rag_2_backend.Models;
using rag_2_backend.Services;

#endregion

namespace rag_2_backend.controllers;

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
    /// <response code="404">Cannot view details</response>
    [HttpGet("{userId:int}/details")]
    [Authorize]
    public UserResponse GetUserDetails(int userId)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new UnauthorizedAccessException("Unauthorized");

        return administrationService.GetUserDetails(email, userId);
    }

    /// <summary>Get students list by study year cycle (Admin, Teacher)</summary>
    [HttpGet("students")]
    [Authorize(Roles = "Admin, Teacher")]
    public List<UserResponse> GetStudents([FromQuery] [Required] int studyCycleYearA,
        [FromQuery] [Required] int studyCycleYearB)
    {
        return administrationService.GetStudents(studyCycleYearA, studyCycleYearB);
    }
}