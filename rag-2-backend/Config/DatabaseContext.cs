using Microsoft.EntityFrameworkCore;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.Config;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public virtual required DbSet<Game> Games { get; init; }
    public virtual required DbSet<RecordedGame> RecordedGames { get; init; }
    public virtual required DbSet<User> Users { get; init; }
    public virtual required DbSet<AccountConfirmationToken> AccountConfirmationTokens { get; init; }
    public virtual required DbSet<BlacklistedJwt> BlacklistedJwts { get; init; }
    public virtual required DbSet<PasswordResetToken> PasswordResetTokens { get; init; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecordedGame>()
            .Property<string>(e=>e.ValuesStr)
            .HasField("_values");
    }
}