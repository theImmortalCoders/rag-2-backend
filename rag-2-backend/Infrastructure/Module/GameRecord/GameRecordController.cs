#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.GameRecord;

[ApiController]
[Route("api/[controller]")]
public class GameRecordController(GameRecordService gameRecordService) : ControllerBase
{
    /// <summary>Get all recorded games for user by game ID and user, admin and teacher can view everyone's data (Auth)</summary>
    /// <response code="404">User or game not found</response>
    [HttpGet]
    [Authorize]
    public List<GameRecordResponse> GetRecordsByGame([Required] int gameId, [Required] int userId)
    {
        var email = AuthDao.GetPrincipalEmail(User);

        return gameRecordService.GetRecordsByGameAndUser(gameId, userId, email);
    }

    /// <summary>Download JSON file from specific game, admin and teacher can download everyone's data (Auth)</summary>
    /// <response code="404">User or game record not found</response>
    /// <response code="403">Permission denied</response>
    /// <response code="400">Record is empty</response>
    [HttpGet("{recordedGameId:int}")]
    [Authorize]
    public FileContentResult DownloadRecordData([Required] int recordedGameId)
    {
        var email = AuthDao.GetPrincipalEmail(User);
        var fileName = "game_record_" + recordedGameId + "_" + email + ".json";
        var fileStream = gameRecordService.DownloadRecordData(recordedGameId, email);

        return File(fileStream, "application/json", fileName);
    }

    /// <summary>Add game recording, limits are present (Auth)</summary>
    /// <response code="404">User or game not found</response>
    /// <response code="400">Space limit exceeded or values state cannot be empty</response>
    [HttpPost]
    [Authorize]
    public void AddGameRecord([FromBody] [Required] GameRecordRequest recordRequest)
    {
        gameRecordService.AddGameRecord(recordRequest, AuthDao.GetPrincipalEmail(User));
    }

    /// <summary>Remove game recording (Auth)</summary>
    /// <response code="404">User or game record not found</response>
    /// <response code="403">Permission denied</response>
    [HttpDelete]
    [Authorize]
    public void RemoveGameRecord([Required] int recordedGameId)
    {
        gameRecordService.RemoveGameRecord(recordedGameId, AuthDao.GetPrincipalEmail(User));
    }
}