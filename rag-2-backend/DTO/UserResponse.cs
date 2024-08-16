using System.ComponentModel.DataAnnotations;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.DTO;

public class UserResponse
{
    [Key] public required int Id { get; set; }
    public required string Email { get; set; }
    public Role Role { get; set; }
    public required int StudyCycleYearA { get; set; }
    public required int StudyCycleYearB { get; set; }
}