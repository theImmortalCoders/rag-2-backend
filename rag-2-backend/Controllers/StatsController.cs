#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO.Stats;
using rag_2_backend.Services;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController(StatsService statsService) : ControllerBase
{
    /// <summary>Get stats for user (Auth)</summary>
    /// <response code="404">User not found</response>
    /// /// <response code="400">Permission denied</response>
    [HttpGet("user")]
    [Authorize]
    public UserStatsResponse GetStatsForUser([Required] [FromQuery] int userId)
    {
        return statsService.GetStatsForUser(UserUtil.GetPrincipalEmail(User), userId);
    }

    /// <summary>Get stats for game</summary>
    /// <response code="404">Game not found</response>
    [HttpGet("game")]
    public GameStatsResponse GetStatsForGame([Required] [FromQuery] int gameId)
    {
        return statsService.GetStatsForGame(gameId);
    }
}