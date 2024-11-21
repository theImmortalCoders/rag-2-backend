#region

using System.Linq.Expressions;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.Administration.Dto;
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

    public List<UserResponse> GetUsers(
        string? email,
        int? studyCycleYearA,
        int? studyCycleYearB,
        string? group,
        string? courseName,
        SortDirection sortDirection,
        UserSortByFields sortBy
    )
    {
        var query = context.Users.Include(u => u.Course).AsQueryable();

        query = FilterUsers(email, studyCycleYearA, studyCycleYearB, group, courseName, query);
        query = SortUsers(sortDirection, sortBy, query);

        return query.AsEnumerable().Select(UserMapper.Map).ToList();
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
            UserSortByFields.Id => ApplySorting(query, x => x.Id, sortDirection),
            UserSortByFields.Email => ApplySorting(query, x => x.Email, sortDirection),
            UserSortByFields.Name => ApplySorting(query, x => x.Name, sortDirection),
            UserSortByFields.StudyYearCycleA => ApplySorting(query, x => x.StudyCycleYearA, sortDirection),
            UserSortByFields.StudyYearCycleB => ApplySorting(query, x => x.StudyCycleYearB, sortDirection),
            UserSortByFields.LastPlayed => ApplySorting(query, x => x.LastPlayed, sortDirection),
            UserSortByFields.CourseName => ApplySorting(query, x => x.Course != null ? x.Course.Name : string.Empty,
                sortDirection),
            UserSortByFields.Group => ApplySorting(query, x => x.Group, sortDirection),
            _ => query
        };
    }

    private static IQueryable<T> ApplySorting<T, TKey>(IQueryable<T> query, Expression<Func<T, TKey>> keySelector,
        SortDirection sortDirection)
    {
        return sortDirection == SortDirection.Desc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}