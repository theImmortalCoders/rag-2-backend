#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class UserDao(DatabaseContext context)
{
    public virtual async Task<User> GetUserByIdOrThrow(int id)
    {
        return await context.Users.SingleOrDefaultAsync(u => u.Id == id) ??
               throw new NotFoundException("User not found");
    }

    public virtual async Task<User> GetUserByEmailOrThrow(string email)
    {
        return await context.Users
                   .Include(u => u.Course)
                   .SingleOrDefaultAsync(u => u.Email == email) ??
               throw new NotFoundException("User not found");
    }

    public virtual async Task<int> CountUsers()
    {
        return await context.Users.CountAsync();
    }
}