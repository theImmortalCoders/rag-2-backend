namespace rag_2_backend.Infrastructure.Module.User.Dto;

public class UserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Name { get; init; }
    public int? StudyCycleYearA { get; init; }
    public int? StudyCycleYearB { get; init; }
}