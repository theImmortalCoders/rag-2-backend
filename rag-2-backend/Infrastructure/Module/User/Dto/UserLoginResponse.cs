namespace rag_2_backend.Infrastructure.Module.User.Dto;

public class UserLoginResponse
{
    public required string JwtToken { get; set; }
    public required string RefreshToken { get; set; }
}