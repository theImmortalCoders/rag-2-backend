using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.models.entity;
using rag_2_backend.Services;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GameRecordController(DatabaseContext context, GameRecordService gameRecordService) : ControllerBase
{
    [HttpGet]
    public IEnumerable<RecordedGameResponse> GetRecordsByGame(int gameId)
    {
        return gameRecordService.GetRecordsByGame(gameId);
    }

    [HttpPost]
    [Authorize]
    public void AddGameRecord([FromBody] RecordedGameRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new KeyNotFoundException("User not found");
        var user = context.Users.SingleOrDefault(u => u.Email == email) ?? throw new KeyNotFoundException("User not found");

        gameRecordService.AddGameRecord(request, user);
    }
}