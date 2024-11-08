#region

using HttpExceptions.Exceptions;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Administration;

public class AdministrationService(DatabaseContext context, UserDao userDao)
{
    public void ChangeBanStatus(int userId, bool isBanned)
    {
        var user = userDao.GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadRequestException("Cannot ban administrator");

        user.Banned = isBanned;
        context.SaveChanges();
    }

    public void ChangeRole(int userId, Role role)
    {
        var user = userDao.GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadRequestException("Cannot change administrator's role");

        user.Role = role;
        context.SaveChanges();
    }

    public UserResponse GetUserDetails(string principalEmail, int userId)
    {
        var principal = userDao.GetUserByEmailOrThrow(principalEmail);

        if (principal.Role is Role.Student && userId != principal.Id)
            throw new ForbiddenException("Cannot view details");

        return UserMapper.Map(userDao.GetUserByIdOrThrow(userId));
    }

    public List<UserResponse> GetStudents()
    {
        return context.Users
            .Select(u => UserMapper.Map(u))
            .ToList();
    }
}