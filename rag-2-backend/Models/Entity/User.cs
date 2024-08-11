using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rag_2_backend.Models.Entity;

[Table("users")]
public class User
{
    [Key] public int Id { get; init; }
    [MaxLength(100)] public required string Email { get; init; }
    [MaxLength(100)] public required string Password { get; init; }
    public Role Role { get; set; } = Role.Student;
}
