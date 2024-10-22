#region

using HttpExceptions.Exceptions;
using rag_2_backend.Config;
using rag_2_backend.DTO.User;
using rag_2_backend.Mapper;
using rag_2_backend.Models;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.Services;

public class AdministrationService(DatabaseContext context, UserUtil userUtil)
{
    public void ChangeBanStatus(int userId, bool isBanned)
    {
        var user = userUtil.GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadRequestException("Cannot ban administrator");

        user.Banned = isBanned;
        context.SaveChanges();
    }

    public void ChangeRole(int userId, Role role)
    {
        var user = userUtil.GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadRequestException("Cannot change administrator's role");

        user.Role = role;
        context.SaveChanges();
    }

    public UserResponse GetUserDetails(string principalEmail, int userId)
    {
        var principal = userUtil.GetUserByEmailOrThrow(principalEmail);

        if (principal.Role is Role.Student or Role.Special && userId != principal.Id)
            throw new ForbiddenException("Cannot view details");

        return UserMapper.MapDetails(userUtil.GetUserByIdOrThrow(userId));
    }

    public List<UserResponse> GetStudents()
    {
        return context.Users
            .Select(u => UserMapper.Map(u))
            .ToList();
    }
}