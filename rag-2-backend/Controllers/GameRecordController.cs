using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO;
using rag_2_backend.Services;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GameRecordController(GameRecordService gameRecordService) : ControllerBase
{
    [HttpGet]
    public List<RecordedGameResponse> GetRecordsByGame([Required] int gameId)
    {
        return gameRecordService.GetRecordsByGame(gameId);
    }

    /// <summary>(Auth)</summary>
    /// <response code="404">User or game not found</response>
    [HttpPost]
    [Authorize]
    public void AddGameRecord([FromBody] [Required] RecordedGameRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new KeyNotFoundException("User not found");

        gameRecordService.AddGameRecord(request, email);
    }
}