using rag_2_backend.data;
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

    //

    private User GetUserByIdOrThrow(int id)
    {
        return context.Users.SingleOrDefault(u => u.Id == id) ??
               throw new KeyNotFoundException("User not found");
    }
}