using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rag_2_backend.Models.Entity;

[Table("users")]
public class User
{
    [Key] public int Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public Role Role { get; set; } = Role.Student;
}
