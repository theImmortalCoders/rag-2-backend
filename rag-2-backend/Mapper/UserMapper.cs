#region

using rag_2_backend.DTO.User;
using rag_2_backend.Models.Entity;

#endregion

namespace rag_2_backend.Mapper;

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
            StudyCycleYearB = user.StudyCycleYearB,
            Name = user.Name,
            LastPlayed = user.LastPlayed.Equals(DateTime.MinValue) ? null : user.LastPlayed
        };
    }
}