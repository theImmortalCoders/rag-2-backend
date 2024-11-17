namespace rag_2_backend.Infrastructure.Module.Auth.Dto;

public class UserLoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required bool RememberMe { get; set; }
}