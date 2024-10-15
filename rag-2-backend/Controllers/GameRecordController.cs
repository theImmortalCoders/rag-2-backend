#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO.RecordedGame;
using rag_2_backend.Services;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GameRecordController(GameRecordService gameRecordService) : ControllerBase
{
    /// <summary>Get all recorded games for user by game ID and user (Auth)</summary>
    /// <response code="404">User or game not found</response>
    [HttpGet]
    [Authorize]
    public List<RecordedGameResponse> GetRecordsByGame([Required] int gameId)
    {
        var email = UserUtil.GetPrincipalEmail(User);

        return gameRecordService.GetRecordsByGameAndUser(gameId, email);
    }

    /// <summary>Download JSON file from specific game, admin and teacher can download everyone's data (Auth)</summary>
    /// <response code="404">User or game record not found</response>
    /// <response code="403">Permission denied</response>
    [HttpGet("{recordedGameId:int}")]
    [Authorize]
    public FileContentResult DownloadRecordData([Required] int recordedGameId)
    {
        var email = UserUtil.GetPrincipalEmail(User);
        var fileName = "game_record_" + recordedGameId + "_" + email + ".json";
        var fileStream = gameRecordService.DownloadRecordData(recordedGameId, email);

        return File(fileStream, "application/json", fileName);
    }

    /// <summary>Add game recording, limits are present (Auth)</summary>
    /// <response code="404">User or game not found</response>
    /// <response code="400">Space limit exceeded</response>
    [HttpPost]
    [Authorize]
    public void AddGameRecord([FromBody] [Required] RecordedGameRequest request)
    {
        gameRecordService.AddGameRecord(request, UserUtil.GetPrincipalEmail(User));
    }

    /// <summary>Remove game recording, admin can remove everyone's data (Auth)</summary>
    /// <response code="404">User or game record not found</response>
    /// <response code="403">Permission denied</response>
    [HttpDelete]
    [Authorize]
    public void RemoveGameRecord([Required] int recordedGameId)
    {
        gameRecordService.RemoveGameRecord(recordedGameId, UserUtil.GetPrincipalEmail(User));
    }
}