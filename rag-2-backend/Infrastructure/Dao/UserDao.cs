#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class UserDao(DatabaseContext context)
{
    public virtual User GetUserByIdOrThrow(int id)
    {
        return context.Users.SingleOrDefault(u => u.Id == id) ??
               throw new NotFoundException("User not found");
    }

    public virtual User GetUserByEmailOrThrow(string email)
    {
        return context.Users
                   .Include(u => u.Course)
                   .SingleOrDefault(u => u.Email == email) ??
               throw new NotFoundException("User not found");
    }

    public virtual int CountUsers()
    {
        return context.Users.Count();
    }
}