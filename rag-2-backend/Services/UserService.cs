using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.Models.Entity;
using rag_2_backend.Utils;

namespace rag_2_backend.Services;

public class UserService(DatabaseContext context, JwtUtil jwtUtil, EmailService emailService)
{
    public void RegisterUser(UserRequest userRequest)
    {
        if (context.Users.Any(u => u.Email == userRequest.Email))
            throw new BadHttpRequestException("User already exists");

        User user = new(userRequest.Email)
        {
            Password = HashUtil.HashPassword(userRequest.Password)
        };
        var token = new AccountConfirmationToken()
        {
            Token = TokenGenerationUtil.CreatePassword(15),
            User = user,
            Expiration = DateTime.Now.AddDays(7)
        };

        context.Users.Add(user);
        context.AccountConfirmationTokens.Add(token);
        context.SaveChanges();

        emailService.SendConfirmationEmail(user.Email, token.Token);
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

    public void LogoutUser(string email)
    {
        // Redis queueing of blacklisted tokens
    }
}