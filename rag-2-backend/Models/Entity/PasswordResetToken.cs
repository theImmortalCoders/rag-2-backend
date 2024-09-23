#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace rag_2_backend.Models.Entity;

[Table("password_reset_token")]
public class PasswordResetToken
{
    [Key] [MaxLength(100)] public required string Token { get; init; }
    public required DateTime Expiration { get; set; }
    public required User User { get; init; }
}