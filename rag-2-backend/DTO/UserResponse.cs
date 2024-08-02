using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.DTO;

public class UserResponse
{
    [Key] public required int Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public Role Role { get; set; }
}
