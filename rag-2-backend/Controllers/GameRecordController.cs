using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.models.entity;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GameRecordController(DatabaseContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecordedGameResponse>>> GetRecordsByGame(int gameId)
    {
        var games = context.RecordedGames.ToArray();
        var records = await context.RecordedGames
            .Include(r => r.Game) //include nullable reference
            .Where(r => r.Game != null && r.Game.Id == gameId)
            .ToListAsync();
        return records.Select(r => new RecordedGameResponse
        {
            Id = r.Id,
            Name = r.Game.Name,
            GameType = r.Game.GameType
        }).ToList();
    }

    [HttpPost]
    [Authorize]
    public void AddGameRecord(int gameId, [FromBody] RecordedGameRequest request)
    {
        var game = context.Games.Find(gameId) ?? throw new KeyNotFoundException("Game not found");
        var recordedGame = new RecordedGame
        {
            Game = game,
            Value = request.Value,
            User = null
        };
        context.RecordedGames.Add(recordedGame);
        context.SaveChanges();
    }
}