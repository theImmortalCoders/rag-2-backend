#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace rag_2_backend.Infrastructure.Database.Entity;

[Table("blacklisted_jwt_table")]
public class BlacklistedJwt
{
    [Key] [MaxLength(500)] public required string Token { get; init; }
    public required DateTime Expiration { get; init; }
}