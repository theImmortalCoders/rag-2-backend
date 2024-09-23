#region

using rag_2_backend.Models;

#endregion

namespace rag_2_backend.DTO.User;

public class UserResponse
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public Role Role { get; set; }
    public required string Name { get; init; }
    public required int StudyCycleYearA { get; set; }
    public required int StudyCycleYearB { get; set; }
}