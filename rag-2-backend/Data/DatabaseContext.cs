using Microsoft.EntityFrameworkCore;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public required DbSet<Game> Games { get; init; }

    public required DbSet<RecordedGame> RecordedGames { get; init; }

    public required DbSet<User> Users { get; init; }
}