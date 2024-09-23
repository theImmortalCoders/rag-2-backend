#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO.Game;
using rag_2_backend.Services;

#endregion

namespace rag_2_backend.controllers;

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
    public void Add([FromBody] [Required] GameRequest request)
    {
        gameService.AddGame(request);
    }

    /// <summary>Edit existing game (Admin)</summary>
    /// <response code="404">Game not found</response>
    /// <response code="400">Game with this name already exists</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public void Edit([FromBody] [Required] GameRequest request, int id)
    {
        gameService.EditGame(request, id);
    }

    /// <summary>Remove existing game (Admin)</summary>
    /// <response code="404">Game not found</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public void Remove(int id)
    {
        gameService.RemoveGame(id);
    }
}