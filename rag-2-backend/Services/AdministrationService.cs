using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.Services;

public class AdministrationService(DatabaseContext context)
{
    public void ChangeBanStatus(int userId, bool isBanned)
    {
        var user = GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadHttpRequestException("Cannot ban administrator");

        user.Banned = isBanned;
        context.SaveChanges();
    }

    public void ChangeRole(int userId, Role role)
    {
        var user = GetUserByIdOrThrow(userId);

        if (user.Role == Role.Admin)
            throw new BadHttpRequestException("Cannot change administrator's role");

        user.Role = role;
        context.SaveChanges();
    }

    public UserDetailsResponse GetUserDetails(string principalEmail, int userId)
    {
        var principal = GetUserByEmailOrThrow(principalEmail);

        if(principal.Role is Role.Student or Role.Special && userId != principal.Id)
            throw new KeyNotFoundException("Cannot view details");

        return UserMapper.MapDetails(GetUserByIdOrThrow(userId));
    }

    public List<UserResponse> GetStudents(int studyCycleYearA, int studyCycleYearB)
    {
        return context.Users
            .Where(u=>u.Role == Role.Student &&
                      u.StudyCycleYearA == studyCycleYearA &&
                      u.StudyCycleYearB == studyCycleYearB)
            .OrderBy(u => u.Email)
            .Select(u => UserMapper.Map(u))
            .ToList();
    }

    //

    private User GetUserByIdOrThrow(int id)
    {
        return context.Users.SingleOrDefault(u => u.Id == id) ??
               throw new KeyNotFoundException("User not found");
    }

    private User GetUserByEmailOrThrow(string email)
    {
        return context.Users.SingleOrDefault(u => u.Email == email) ??
               throw new KeyNotFoundException("User not found");
    }
}