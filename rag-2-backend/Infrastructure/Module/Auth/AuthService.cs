#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Auth.Dto;
using rag_2_backend.Infrastructure.Module.User.Dto;
using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Infrastructure.Module.Auth;

public class AuthService(
    UserDao userDao,
    RefreshTokenDao refreshTokenDao,
    DatabaseContext databaseContext,
    JwtUtil jwtUtil
)
{
    public async Task<UserLoginResponse> LoginUser(string email, string password, double refreshTokenExpirationTimeDays)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);

        if (!HashUtil.VerifyPassword(password, user.Password))
            throw new UnauthorizedException("Invalid password");
        if (!user.Confirmed)
            throw new UnauthorizedException("Mail not confirmed");
        if (user.Banned)
            throw new UnauthorizedException("User banned");

        var refreshToken = await GenerateRefreshToken(refreshTokenExpirationTimeDays, user);

        return new UserLoginResponse
        {
            JwtToken = jwtUtil.GenerateJwt(user.Email, user.Role.ToString()),
            RefreshToken = refreshToken.Token
        };
    }

    public async Task<string> RefreshToken(string refreshToken)
    {
        var token = await databaseContext.RefreshTokens
                        .Include(t => t.User)
                        .SingleOrDefaultAsync(t => t.Token == refreshToken && t.Expiration > DateTime.Now)
                    ?? throw new UnauthorizedException("Invalid refresh token");
        var user = token.User;

        return jwtUtil.GenerateJwt(user.Email, user.Role.ToString());
    }

    public async Task<UserResponse> GetMe(string email)
    {
        return UserMapper.Map(await userDao.GetUserByEmailOrThrow(email));
    }

    public async Task LogoutUser(string token)
    {
        await refreshTokenDao.RemoveTokenByToken(token);
    }

    //

    private async Task<RefreshToken> GenerateRefreshToken(double refreshTokenExpirationTimeDays,
        Database.Entity.User user)
    {
        databaseContext.Attach(user);
        var refreshToken = new RefreshToken
        {
            User = user,
            Expiration = DateTime.Now.AddDays(refreshTokenExpirationTimeDays),
            Token = Guid.NewGuid().ToString()
        };
        await databaseContext.RefreshTokens.AddAsync(refreshToken);
        await databaseContext.SaveChangesAsync();
        return refreshToken;
    }
}