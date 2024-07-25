using Microsoft.EntityFrameworkCore;
using rag_2_backend.models.entity;

namespace rag_2_backend.data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public required DbSet<Game> Games { get; init; }

    public required DbSet<RecordedGame> RecordedGames { get; init; }
}