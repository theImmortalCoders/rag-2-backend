#region

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using rag_2_backend.Models;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;

#endregion

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
        var recordedGameValueComparer = new ValueComparer<List<RecordedGameValue>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        var playerComparer = new ValueComparer<List<Player>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<RecordedGame>()
            .Property(e => e.Values)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<RecordedGameValue>>(v, (JsonSerializerOptions)null!)!)
            .Metadata.SetValueComparer(recordedGameValueComparer);

        modelBuilder.Entity<RecordedGame>()
            .Property(e => e.Players)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<Player>>(v, (JsonSerializerOptions)null!)!)
            .Metadata.SetValueComparer(playerComparer);
    }
}