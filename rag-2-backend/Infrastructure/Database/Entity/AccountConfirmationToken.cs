#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace rag_2_backend.Infrastructure.Database.Entity;

[Table("account_confirmation_token_table")]
public class AccountConfirmationToken
{
    [Key] [MaxLength(100)] public required string Token { get; init; }
    public required DateTime Expiration { get; set; }

    [ForeignKey("UserId")] public required User User { get; init; }
}