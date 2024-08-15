using Microsoft.EntityFrameworkCore;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public virtual required DbSet<Game> Games { get; init; }

    public virtual required DbSet<RecordedGame> RecordedGames { get; init; }

    public virtual required DbSet<User> Users { get; init; }
    public virtual required DbSet<AccountConfirmationToken> AccountConfirmationTokens { get; init; }
}