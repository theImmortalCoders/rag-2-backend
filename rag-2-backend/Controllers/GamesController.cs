using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.models.entity;
using rag_2_backend.Utils;

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
}