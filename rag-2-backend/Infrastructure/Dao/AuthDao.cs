#region

using System.Security.Claims;
using HttpExceptions.Exceptions;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public static class AuthDao
{
    public static string GetPrincipalEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? throw new UnauthorizedException("Unauthorized");
    }
}