using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rag_2_backend.Models.Entity;

[Table("account_confirmation_token")]
public class AccountConfirmationToken
{
    [Key] [MaxLength(100)] public required string Token { get; init; }
    public required DateTime Expiration { get; set; }
    public required User User { get; init; }
}