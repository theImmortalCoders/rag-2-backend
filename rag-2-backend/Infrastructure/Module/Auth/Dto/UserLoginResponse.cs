namespace rag_2_backend.Infrastructure.Module.Auth.Dto;

public class UserLoginResponse
{
    public required string JwtToken { get; init; }
    public required string RefreshToken { get; init; }
}