#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.Stats.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Stats;

[ApiController]
[Route("api/[controller]")]
public class StatsController(StatsService statsService) : ControllerBase
{
    /// <summary>Get stats for user, Student can only view his data (Auth)</summary>
    /// <response code="404">User not found</response>
    /// <response code="403">Permission denied</response>
    [HttpGet("user")]
    [Authorize]
    public UserStatsResponse GetStatsForUser([Required] [FromQuery] int userId)
    {
        return statsService.GetStatsForUser(AuthDao.GetPrincipalEmail(User), userId);
    }

    /// <summary>Get stats for game</summary>
    /// <response code="404">Game not found</response>
    [HttpGet("game")]
    public GameStatsResponse GetStatsForGame([Required] [FromQuery] int gameId)
    {
        return statsService.GetStatsForGame(gameId);
    }

    /// <summary>Get overall stats</summary>
    [HttpGet("all")]
    public OverallStatsResponse GetOverallStats()
    {
        return statsService.GetOverallStats();
    }
}