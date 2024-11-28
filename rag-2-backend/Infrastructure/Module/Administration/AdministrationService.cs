#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.Administration.Dto;
using rag_2_backend.Infrastructure.Module.User.Dto;
using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Infrastructure.Module.Administration;

public class AdministrationService(DatabaseContext context, UserDao userDao)
{
    public async Task ChangeBanStatus(int userId, bool isBanned)
    {
        var user = await userDao.GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadRequestException("Cannot ban administrator");

        user.Banned = isBanned;
        await context.SaveChangesAsync();
    }

    public async Task ChangeRole(int userId, Role role)
    {
        var user = await userDao.GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadRequestException("Cannot change administrator's role");

        user.Role = role;
        await context.SaveChangesAsync();
    }

    public async Task<UserResponse> GetUserDetails(string principalEmail, int userId)
    {
        var principal = await userDao.GetUserByEmailOrThrow(principalEmail);

        if (principal.Role is Role.Student && userId != principal.Id)
            throw new ForbiddenException("Cannot view details");

        return UserMapper.Map(await userDao.GetUserByIdOrThrow(userId));
    }

    public async Task<List<UserResponse>> GetUsers(
        Role role,
        string? email,
        int? studyCycleYearA,
        int? studyCycleYearB,
        string? group,
        string? courseName,
        SortDirection sortDirection,
        UserSortByFields sortBy
    )
    {
        var query = context.Users
            .Include(u => u.Course)
            .Where(u => u.Role == role)
            .AsQueryable();

        query = FilterUsers(email, studyCycleYearA, studyCycleYearB, group, courseName, query);
        query = SortUsers(sortDirection, sortBy, query);

        return await Task.Run(() => query.AsEnumerable().Select(UserMapper.Map).ToList());
    }

    //

    private static IQueryable<Database.Entity.User> FilterUsers(string? email, int? studyCycleYearA,
        int? studyCycleYearB, string? group,
        string? courseName, IQueryable<Database.Entity.User> query)
    {
        if (!string.IsNullOrEmpty(email))
            query = query.Where(u => u.Email.ToLower().Contains(email.ToLower()));
        if (studyCycleYearA.HasValue)
            query = query.Where(u => u.StudyCycleYearA == studyCycleYearA.Value);
        if (studyCycleYearB.HasValue)
            query = query.Where(u => u.StudyCycleYearB == studyCycleYearB.Value);
        if (!string.IsNullOrEmpty(group))
            query = query.Where(u => u.Group != null && u.Group.ToLower().Contains(
                group.ToLower()
            ));
        if (!string.IsNullOrEmpty(courseName))
            query = query.Where(u => u.Course != null && u.Course.Name.Equals(courseName));
        return query;
    }

    private static IQueryable<Database.Entity.User> SortUsers(SortDirection sortDirection, UserSortByFields sortBy,
        IQueryable<Database.Entity.User> query)
    {
        return sortBy switch
        {
            UserSortByFields.Id => DataSortingUtil.ApplySorting(query, x => x.Id, sortDirection),
            UserSortByFields.Email => DataSortingUtil.ApplySorting(query, x => x.Email, sortDirection),
            UserSortByFields.Name => DataSortingUtil.ApplySorting(query, x => x.Name, sortDirection),
            UserSortByFields.StudyYearCycleA => DataSortingUtil.ApplySorting(query, x => x.StudyCycleYearA,
                sortDirection),
            UserSortByFields.StudyYearCycleB => DataSortingUtil.ApplySorting(query, x => x.StudyCycleYearB,
                sortDirection),
            UserSortByFields.LastPlayed => DataSortingUtil.ApplySorting(query, x => x.LastPlayed, sortDirection),
            UserSortByFields.CourseName => DataSortingUtil.ApplySorting(query,
                x => x.Course != null ? x.Course.Name : string.Empty,
                sortDirection),
            UserSortByFields.Group => DataSortingUtil.ApplySorting(query, x => x.Group, sortDirection),
            _ => query
        };
    }
}