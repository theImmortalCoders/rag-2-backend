#region

using System.IdentityModel.Tokens.Jwt;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Config;
using rag_2_backend.DTO.User;
using rag_2_backend.Mapper;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.Services;

public class UserService(
    DatabaseContext context,
    JwtUtil jwtUtil,
    EmailService emailService,
    JwtSecurityTokenHandler jwtSecurityTokenHandler,
    UserUtil userUtil)
{
    public void RegisterUser(UserRequest userRequest)
    {
        if (context.Users.Any(u => u.Email == userRequest.Email))
            throw new BadRequestException("User already exists");

        User user = new(userRequest.Email)
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
        var user = userUtil.GetUserByEmailOrThrow(email);
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

    public string LoginUser(string email, string password)
    {
        var user = userUtil.GetUserByEmailOrThrow(email);

        if (!HashUtil.VerifyPassword(password, user.Password))
            throw new UnauthorizedException("Invalid password");
        if (!user.Confirmed)
            throw new UnauthorizedException("Mail not confirmed");
        if (user.Banned)
            throw new UnauthorizedException("User banned");

        return jwtUtil.GenerateToken(user.Email, user.Role.ToString());
    }

    public UserResponse GetMe(string email)
    {
        return UserMapper.Map(userUtil.GetUserByEmailOrThrow(email));
    }

    public void LogoutUser(string header)
    {
        var tokenValue = header["Bearer ".Length..].Trim();
        var jwtToken = jwtSecurityTokenHandler.ReadToken(tokenValue) as JwtSecurityToken ??
                       throw new UnauthorizedException("Unauthorized");

        context.BlacklistedJwts.Add(new BlacklistedJwt { Token = tokenValue, Expiration = jwtToken.ValidTo });
        context.SaveChanges();
    }

    public void RequestPasswordReset(string email)
    {
        var user = userUtil.GetUserByEmailOrThrow(email);

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
        var user = userUtil.GetUserByEmailOrThrow(email);

        if (!HashUtil.VerifyPassword(oldPassword, user.Password))
            throw new BadRequestException("Invalid old password");
        if (user.Password == HashUtil.HashPassword(newPassword))
            throw new BadRequestException("Password cannot be same");

        user.Password = HashUtil.HashPassword(newPassword);
        context.SaveChanges();
    }

    public void DeleteAccount(string email, string header)
    {
        var user = userUtil.GetUserByEmailOrThrow(email);

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

    private void GenerateAccountTokenAndSendConfirmationMail(User user)
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

    private void GeneratePasswordResetTokenAndSendMail(User user)
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