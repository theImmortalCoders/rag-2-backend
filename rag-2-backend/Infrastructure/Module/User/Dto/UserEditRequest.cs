namespace rag_2_backend.Infrastructure.Module.User.Dto;

public class UserEditRequest
{
    public required string Name { get; init; }
    public int? StudyCycleYearA { get; init; }
    public int? StudyCycleYearB { get; init; }
    public int? CourseId { get; init; }
    public string? Group { get; init; }
}