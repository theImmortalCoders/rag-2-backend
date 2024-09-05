using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rag_2_backend.Models.Entity;

[Table("blacklisted_jwt")]
public class BlacklistedJwt
{
    [Key] [MaxLength(500)] public required string Token { get; init; }
    public required DateTime Expiration { get; init; }
}