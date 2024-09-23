#region

using System.Security.Claims;
using HttpExceptions.Exceptions;
using rag_2_backend.Config;
using rag_2_backend.Models.Entity;

#endregion

namespace rag_2_backend.Utils;

public class UserUtil(DatabaseContext context)
{
    public virtual User GetUserByIdOrThrow(int id)
    {
        return context.Users.SingleOrDefault(u => u.Id == id) ??
               throw new NotFoundException("User not found");
    }

    public virtual User GetUserByEmailOrThrow(string email)
    {
        return context.Users.SingleOrDefault(u => u.Email == email) ??
               throw new NotFoundException("User not found");
    }

    public static string GetPrincipalEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? throw new UnauthorizedException("Unauthorized");
    }
}