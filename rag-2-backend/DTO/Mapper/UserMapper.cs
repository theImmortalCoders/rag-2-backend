namespace rag_2_backend.DTO.Mapper;
using rag_2_backend.DTO;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;

public class UserMapper
{
    public static UserResponse Map(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Password = user.Password,
            Role = user.Role
        };
    }

    public static User Map(UserResponse userResponse)
    {
        return new User
        {
            Id = userResponse.Id,
            Email = userResponse.Email,
            Password = userResponse.Password,
            Role = userResponse.Role
        };
    }
}
