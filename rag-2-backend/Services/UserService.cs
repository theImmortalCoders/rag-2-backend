using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.Models.Entity;
using rag_2_backend.Utils;

namespace rag_2_backend.Services;

public class UserService(DatabaseContext context, JwtUtil jwtUtil)
{
    public async void RegisterUser(UserRequest userRequest)
    {
        if (context.Users.Any(u => u.Email == userRequest.Email))
            throw new ArgumentException("User already exists");


        User user = new()
        {
            Email = userRequest.Email,
            Password = HashUtil.HashPassword(userRequest.Password)
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task<UserResponse> GetMe(string email)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new KeyNotFoundException("User not found");

        return UserMapper.Map(user);
    }

    public async Task<string> LoginUser(string email, string password)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new KeyNotFoundException("User not found");

        if (!HashUtil.VerifyPassword(password, user.Password))
            throw new UnauthorizedAccessException("Invalid password");

        return jwtUtil.GenerateToken(user.Email, user.Role.ToString());
    }

    public void LogoutUser(string email)
    {
        // Redis queueing of blackisted tokens
    }
}

