using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.Models.Entity;
using rag_2_backend.Utils;

namespace rag_2_backend.Services;

public class UserService(DatabaseContext context, JwtUtil jwtUtil, EmailSendingUtil emailSendingUtil)
{
    public void RegisterUser(UserRequest userRequest)
    {
        if (context.Users.Any(u => u.Email == userRequest.Email))
            throw new BadHttpRequestException("User already exists");

        User user = new(userRequest.Email)
        {
            Password = HashUtil.HashPassword(userRequest.Password)
        };

        context.Users.Add(user);
        context.SaveChanges();

        Task.Run(async () =>
            await emailSendingUtil.SendMail("marcinbator.ofc@gmail.com", "Marcinbator Ofc", user.Email));
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