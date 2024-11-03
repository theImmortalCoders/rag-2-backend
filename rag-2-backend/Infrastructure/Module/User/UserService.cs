#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Email;
using rag_2_backend.Infrastructure.Module.User.Dto;
using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Infrastructure.Module.User;

public class UserService(
    DatabaseContext context,
    JwtUtil jwtUtil,
    EmailService emailService,
    UserDao userDao,
    RefreshTokenDao refreshTokenDao)
{
    public void RegisterUser(UserRequest userRequest)
    {
        if (context.Users.Any(u => u.Email == userRequest.Email))
            throw new BadRequestException("User already exists");

        Database.Entity.User user = new(userRequest.Email)
        {
            Name = userRequest.Name,
            Password = HashUtil.HashPassword(userRequest.Password),
            StudyCycleYearA = userRequest.StudyCycleYearA,
            StudyCycleYearB = userRequest.StudyCycleYearB
        };

        if (user.Role == Role.Student && IsStudyYearWrong(userRequest))
            throw new BadRequestException("Wrong study cycle year");

        context.Users.Add(user);
        GenerateAccountTokenAndSendConfirmationMail(user);
        context.SaveChanges();
    }

    public void ResendConfirmationEmail(string email)
    {
        var user = userDao.GetUserByEmailOrThrow(email);
        if (user.Confirmed) throw new BadRequestException("User is already confirmed");

        context.AccountConfirmationTokens.RemoveRange(
            context.AccountConfirmationTokens.Where(a => a.User.Email == user.Email)
        );

        GenerateAccountTokenAndSendConfirmationMail(user);
        context.SaveChanges();
    }

    public void ConfirmAccount(string tokenValue)
    {
        var token = context.AccountConfirmationTokens
                        .Include(t => t.User)
                        .SingleOrDefault(t => t.Token == tokenValue)
                    ?? throw new BadRequestException("Invalid token");

        if (token.Expiration < DateTime.Now) throw new BadRequestException("Invalid token");

        token.User.Confirmed = true;
        context.AccountConfirmationTokens.Remove(token);
        context.SaveChanges();
    }

    public UserLoginResponse LoginUser(string email, string password, double refreshTokenExpirationTimeDays)
    {
        var user = userDao.GetUserByEmailOrThrow(email);

        if (!HashUtil.VerifyPassword(password, user.Password))
            throw new UnauthorizedException("Invalid password");
        if (!user.Confirmed)
            throw new UnauthorizedException("Mail not confirmed");
        if (user.Banned)
            throw new UnauthorizedException("User banned");

        var refreshToken = new RefreshToken
        {
            User = user,
            Expiration = DateTime.Now.AddDays(refreshTokenExpirationTimeDays),
            Token = Guid.NewGuid().ToString()
        };
        refreshTokenDao.RemoveTokensForUser(user);
        context.RefreshTokens.Add(refreshToken);
        context.SaveChanges();

        return new UserLoginResponse
        {
            JwtToken = jwtUtil.GenerateToken(user.Email, user.Role.ToString()),
            RefreshToken = refreshToken.Token
        };
    }

    public string RefreshToken(string refreshToken)
    {
        var token = context.RefreshTokens
                        .Include(t => t.User)
                        .SingleOrDefault(t => t.Token == refreshToken && t.Expiration > DateTime.Now)
                    ?? throw new UnauthorizedException("Invalid refresh token");
        var user = token.User;

        return jwtUtil.GenerateToken(user.Email, user.Role.ToString());
    }

    public UserResponse GetMe(string email)
    {
        return UserMapper.Map(userDao.GetUserByEmailOrThrow(email));
    }

    public void LogoutUser(string email)
    {
        var user = userDao.GetUserByEmailOrThrow(email);
        refreshTokenDao.RemoveTokensForUser(user);
    }

    public void RequestPasswordReset(string email)
    {
        var user = userDao.GetUserByEmailOrThrow(email);

        context.PasswordResetTokens.RemoveRange(
            context.PasswordResetTokens.Where(a => a.User.Email == user.Email)
        );

        GeneratePasswordResetTokenAndSendMail(user);
        context.SaveChanges();
    }

    public void ResetPassword(string tokenValue, string newPassword)
    {
        var token = context.PasswordResetTokens
                        .Include(t => t.User)
                        .SingleOrDefault(t => t.Token == tokenValue)
                    ?? throw new BadRequestException("Invalid token");

        if (token.Expiration < DateTime.Now) throw new BadRequestException("Invalid token");

        token.User.Password = HashUtil.HashPassword(newPassword);
        context.PasswordResetTokens.Remove(token);
        context.SaveChanges();
    }

    public void ChangePassword(string email, string oldPassword, string newPassword)
    {
        var user = userDao.GetUserByEmailOrThrow(email);

        if (!HashUtil.VerifyPassword(oldPassword, user.Password))
            throw new BadRequestException("Invalid old password");
        if (user.Password == HashUtil.HashPassword(newPassword))
            throw new BadRequestException("Password cannot be same");

        user.Password = HashUtil.HashPassword(newPassword);
        context.SaveChanges();
    }

    public void DeleteAccount(string email, string header)
    {
        var user = userDao.GetUserByEmailOrThrow(email);

        context.PasswordResetTokens.RemoveRange(context.PasswordResetTokens
            .Include(p => p.User)
            .Where(a => a.User.Email == email)
        );
        context.AccountConfirmationTokens.RemoveRange(context.AccountConfirmationTokens
            .Include(p => p.User)
            .Where(a => a.User.Email == email)
        );
        context.RecordedGames.RemoveRange(context.RecordedGames
            .Include(p => p.User)
            .Where(a => a.User.Email == email)
        );

        context.Users.Remove(user);
        context.SaveChanges();

        LogoutUser(header);
    }

    //

    private static bool IsStudyYearWrong(UserRequest userRequest)
    {
        return userRequest.StudyCycleYearA == 0 || userRequest.StudyCycleYearB == 0 ||
               userRequest.StudyCycleYearB - userRequest.StudyCycleYearA != 1;
    }

    private void GenerateAccountTokenAndSendConfirmationMail(Database.Entity.User user)
    {
        var token = new AccountConfirmationToken
        {
            Token = TokenGenerationUtil.GenerateToken(15),
            User = user,
            Expiration = DateTime.Now.AddDays(7)
        };
        context.AccountConfirmationTokens.Add(token);
        emailService.SendConfirmationEmail(user.Email, token.Token);
    }

    private void GeneratePasswordResetTokenAndSendMail(Database.Entity.User user)
    {
        var token = new PasswordResetToken
        {
            Token = TokenGenerationUtil.GenerateToken(15),
            User = user,
            Expiration = DateTime.Now.AddDays(2)
        };
        context.PasswordResetTokens.Add(token);
        emailService.SendPasswordResetMail(user.Email, token.Token);
    }
}