namespace rag_2_backend.DTO.Mapper;
using rag_2_backend.DTO;
using rag_2_backend.Models.Entity;

public class UserMapper
{
    public static UserResponse Map(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }
}
