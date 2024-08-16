using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO;
using rag_2_backend.Services;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(GameService gameService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<GameResponse>> GetGames()
    {
        return await gameService.GetGames();
    }

    /// <summary>
    /// (Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public void Add([FromBody] [Required] GameRequest request)
    {
        gameService.AddGame(request);
    }

    /// <summary>
    /// (Admin)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public void Edit([FromBody] [Required] GameRequest request, int id)
    {
        gameService.EditGame(request, id);
    }

    /// <summary>
    /// (Admin)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public void Remove(int id)
    {
        gameService.RemoveGame(id);
    }
}