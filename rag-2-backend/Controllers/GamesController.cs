using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.models;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(DatabaseContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await context.Users.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user =  await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return user;
    }
}