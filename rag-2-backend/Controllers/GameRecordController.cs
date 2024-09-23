#region

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO.RecordedGame;
using rag_2_backend.Services;

#endregion

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GameRecordController(GameRecordService gameRecordService) : ControllerBase
{
    /// <summary>Get all recorded games for user by gameId (Auth)</summary>
    /// <response code="404">User or game not found</response>
    [HttpGet]
    public List<RecordedGameResponse> GetRecordsByGame([Required] int gameId)
    {
        return gameRecordService.GetRecordsByGame(gameId);
    }

    /// <summary>Download JSON file from specific game (Auth)</summary>
    /// <response code="404">User or game record not found</response>
    /// <response code="400">Permission denied</response>
    [HttpGet("{recordedGameId:int}")]
    public FileContentResult DownloadRecordData([Required] int recordedGameId)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new KeyNotFoundException("User not found");
        var fileName = "game_record_" + recordedGameId + "_" + email + ".json";
        var fileStream = gameRecordService.DownloadRecordData(recordedGameId, email);

        return File(fileStream, "application/json", fileName);
    }

    /// <summary>Add game recording (Auth)</summary>
    /// <response code="404">User or game not found</response>
    /// <response code="400">Space limit exceeded</response>
    [HttpPost]
    [Authorize]
    public void AddGameRecord([FromBody] [Required] RecordedGameRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new KeyNotFoundException("User not found");

        gameRecordService.AddGameRecord(request, email);
    }

    /// <summary>Remove game recording (Auth)</summary>
    /// <response code="404">User or game record not found</response>
    /// <response code="400">Permission denied</response>
    [HttpDelete]
    [Authorize]
    public void RemoveGameRecord([Required] int recordedGameId)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new KeyNotFoundException("User not found");

        gameRecordService.RemoveGameRecord(recordedGameId, email);
    }

    /// <summary>Get all used space for user (Auth)</summary>
    [HttpGet("used-space")]
    [Authorize]
    public double GetUsedSpace()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new KeyNotFoundException("User not found");

        return gameRecordService.GetCurrentSpaceByUser(email);
    }
}