#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Module.Game.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Game;

[ApiController]
[Route("api/[controller]")]
public class GameController(GameService gameService) : ControllerBase
{
    /// <summary>Get games available in system</summary>
    [HttpGet]
    public async Task<IEnumerable<GameResponse>> GetGames()
    {
        return await gameService.GetGames();
    }

    /// <summary>Add new game to system (Admin)</summary>
    /// <response code="400">Game with this name already exists</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task Add([FromBody] [Required] GameRequest request)
    {
        await gameService.AddGame(request);
    }

    /// <summary>Edit existing game (Admin)</summary>
    /// <response code="404">Game not found</response>
    /// <response code="400">Game with this name already exists</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task Edit([FromBody] [Required] GameRequest request, int id)
    {
        await gameService.EditGame(request, id);
    }

    /// <summary>Remove existing game (Admin)</summary>
    /// <response code="404">Game not found</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task Remove([Required] int id)
    {
        await gameService.RemoveGame(id);
    }
}