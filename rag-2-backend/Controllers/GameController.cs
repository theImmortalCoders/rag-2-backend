using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO;
using rag_2_backend.Services;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController(GameService _gameService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<GameResponse>> GetGames()
    {
        return await _gameService.GetGames();
    }

    /// <summary>
    /// (Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public void Add([FromBody][Required] GameRequest request)
    {
        _gameService.AddGame(request);
    }
}