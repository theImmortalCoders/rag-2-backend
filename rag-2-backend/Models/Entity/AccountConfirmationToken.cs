using System.ComponentModel.DataAnnotations;

namespace rag_2_backend.Models.Entity;

public class AccountConfirmationToken
{
    [Key] [MaxLength(100)] public required string Token { get; init; }
    public required DateTime Expiration { get; init; }
    public required User User { get; init; }
}