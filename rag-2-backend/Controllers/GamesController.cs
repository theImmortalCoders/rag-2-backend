using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.models.entity;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(DatabaseContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Game>>> GetGames()
    {
        return await context.Games.ToListAsync();
    }

    [HttpPost]
    public void AddGameRecord(int gameId, [FromBody] RecordedGameRequest request)
    {
        var game = context.Games.Find(gameId);
        if(game == null)
        {
            throw new ArgumentException("Game not found");
        }
        var recordedGame = new RecordedGame
        {
            Game = game,
            Value = request.Value
        };
        context.RecordedGames.Add(recordedGame);
        context.SaveChanges();
    }
}