using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;
using rag_2_backend.Utils;

namespace rag_2_backend.Services;

public class UserService(
    DatabaseContext context,
    JwtUtil jwtUtil,
    EmailService emailService,
    JwtSecurityTokenHandler jwtSecurityTokenHandler)
{
    public void RegisterUser(UserRequest userRequest)
    {
        if (context.Users.Any(u => u.Email == userRequest.Email))
            throw new BadHttpRequestException("User already exists");

        User user = new(userRequest.Email)
        {
            Password = HashUtil.HashPassword(userRequest.Password),
            StudyCycleYearA = userRequest.StudyCycleYearA,
            StudyCycleYearB = userRequest.StudyCycleYearB,
        };
        if (user.Role == Role.Student)
        {
            if (userRequest.StudyCycleYearA == 0 || userRequest.StudyCycleYearB == 0 ||
                userRequest.StudyCycleYearB - userRequest.StudyCycleYearA != 1)
                throw new BadHttpRequestException("Wrong study cycle year");
        }

        context.Users.Add(user);

        GenerateAccountTokenAndSendConfirmationMail(user);

        context.SaveChanges();
    }

    public void ResendConfirmationEmail(string email)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email) ??
                   throw new KeyNotFoundException("User not found");

        if (user.Confirmed) throw new BadHttpRequestException("User is already confirmed");

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
                    ?? throw new BadHttpRequestException("Invalid token");
        if (token.Expiration < DateTime.Now) throw new BadHttpRequestException("Invalid token");

        var user = context.Users.SingleOrDefault(u => u.Email == token.User.Email) ??
                   throw new KeyNotFoundException("User not found");
        user.Confirmed = true;

        context.AccountConfirmationTokens.Remove(token);
        context.SaveChanges();
    }

    public async Task<string> LoginUser(string email, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email) ??
                   throw new KeyNotFoundException("User not found");

        if (!HashUtil.VerifyPassword(password, user.Password))
            throw new UnauthorizedAccessException("Invalid password");
        if (!user.Confirmed)
            throw new UnauthorizedAccessException("Confirm email");

        return jwtUtil.GenerateToken(user.Email, user.Role.ToString());
    }

    public async Task<UserResponse> GetMe(string email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email) ??
                   throw new KeyNotFoundException("User not found");

        return UserMapper.Map(user);
    }

    public void LogoutUser(string header)
    {
        var tokenValue = header["Bearer ".Length..].Trim();

        var jwtToken = jwtSecurityTokenHandler.ReadToken(tokenValue) as JwtSecurityToken ??
                       throw new UnauthorizedAccessException("Unauthorized");

        var expiryDate = jwtToken.ValidTo;
        context.BlacklistedJwts.Add(new BlacklistedJwt { Token = tokenValue, Expiration = expiryDate });

        context.SaveChanges();
    }

    public void RequestPasswordReset(string email)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email) ??
                   throw new KeyNotFoundException("User not found");

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
                    ?? throw new BadHttpRequestException("Invalid token");
        if (token.Expiration < DateTime.Now) throw new BadHttpRequestException("Invalid token");

        var user = context.Users.SingleOrDefault(u => u.Email == token.User.Email) ??
                   throw new KeyNotFoundException("User not found");
        user.Password = HashUtil.HashPassword(newPassword);

        context.PasswordResetTokens.Remove(token);
        context.SaveChanges();
    }

    public void ChangePassword(string email, string oldPassword, string newPassword)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email) ??
                   throw new KeyNotFoundException("User not found");

        if (!HashUtil.VerifyPassword(oldPassword, user.Password))
            throw new BadHttpRequestException("Invalid old password");
        if (user.Password == HashUtil.HashPassword(newPassword))
            throw new BadHttpRequestException("Password cannot be same");

        user.Password = HashUtil.HashPassword(newPassword);
        context.SaveChanges();
    }

    public void DeleteAccount(string email, string header)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email) ??
                   throw new KeyNotFoundException("User not found");

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
            Expiration = DateTime.Now.AddDays(7)
        };
        context.PasswordResetTokens.Add(token);
        emailService.SendPasswordResetMail(user.Email, token.Token);
    }
}