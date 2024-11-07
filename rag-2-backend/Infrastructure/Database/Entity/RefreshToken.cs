#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace rag_2_backend.Infrastructure.Database.Entity;

[Table("refresh_token_table")]
public class RefreshToken
{
    [Key] [MaxLength(100)] public required string Token { get; init; }
    public required DateTime Expiration { get; init; }
    public required User User { get; init; }
}