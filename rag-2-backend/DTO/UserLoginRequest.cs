namespace rag_2_backend.DTO;

public class UserLoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}