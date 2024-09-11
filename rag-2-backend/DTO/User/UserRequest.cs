namespace rag_2_backend.DTO;

public class UserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Name { get; init; }
    public int StudyCycleYearA { get; init; }
    public int StudyCycleYearB { get; init; }
}