using rag_2_backend.Models.Entity;

namespace rag_2_backend.DTO.Mapper;

public abstract class UserMapper
{
    public static UserResponse Map(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            StudyCycleYearA = user.StudyCycleYearA,
            StudyCycleYearB = user.StudyCycleYearB
        };
    }
}