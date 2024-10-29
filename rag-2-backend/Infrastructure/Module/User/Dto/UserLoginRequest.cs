namespace rag_2_backend.Infrastructure.Module.User.Dto;

public class UserLoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}