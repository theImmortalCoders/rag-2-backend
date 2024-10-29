#region

using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Common.Mapper;

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
            LastPlayed = user.LastPlayed.Equals(DateTime.MinValue) ? null : user.LastPlayed,
            Banned = user.Banned
        };
    }
}