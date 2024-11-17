#region

using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Module.User.Dto;

public class UserResponse
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public Role Role { get; set; }
    public required string Name { get; init; }
    public required int StudyCycleYearA { get; set; }
    public required int StudyCycleYearB { get; set; }
    public DateTime? LastPlayed { get; set; }
    public bool Banned { get; set; }
    public Course? Course { get; set; }
    public string? Group { get; set; }
}