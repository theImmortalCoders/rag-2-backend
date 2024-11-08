#region

using System.Security.Claims;
using HttpExceptions.Exceptions;
using rag_2_backend.Infrastructure.Dao;
using Xunit;

#endregion

namespace rag_2_backend.Test.Dao;

public class AuthDaoTests
{
    private static ClaimsPrincipal CreateClaimsPrincipalWithClaims(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void GetPrincipalEmail_ShouldReturnEmail_WhenEmailClaimExists()
    {
        const string email = "test@example.com";
        var principal = CreateClaimsPrincipalWithClaims(new Claim(ClaimTypes.Email, email));

        var result = AuthDao.GetPrincipalEmail(principal);

        Assert.Equal(email, result);
    }

    [Fact]
    public void GetPrincipalEmail_ShouldThrowUnauthorizedException_WhenEmailClaimDoesNotExist()
    {
        var principal = CreateClaimsPrincipalWithClaims();

        Assert.Throws<UnauthorizedException>(() => AuthDao.GetPrincipalEmail(principal));
    }
}