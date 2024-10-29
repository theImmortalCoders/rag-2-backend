#region

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public virtual required DbSet<Game> Games { get; init; }
    public virtual required DbSet<GameRecord> RecordedGames { get; init; }
    public virtual required DbSet<User> Users { get; init; }
    public virtual required DbSet<AccountConfirmationToken> AccountConfirmationTokens { get; init; }
    public virtual required DbSet<BlacklistedJwt> BlacklistedJwts { get; init; }
    public virtual required DbSet<PasswordResetToken> PasswordResetTokens { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var recordedGameValueComparer = new ValueComparer<List<GameRecordValue>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        var playerComparer = new ValueComparer<List<Player>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<GameRecord>()
            .Property(e => e.Values)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<GameRecordValue>>(v, (JsonSerializerOptions)null!)!)
            .Metadata.SetValueComparer(recordedGameValueComparer);

        modelBuilder.Entity<GameRecord>()
            .Property(e => e.Players)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<Player>>(v, (JsonSerializerOptions)null!)!)
            .Metadata.SetValueComparer(playerComparer);
    }
}