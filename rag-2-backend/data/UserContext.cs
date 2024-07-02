using Microsoft.EntityFrameworkCore;
using rag_2_backend.models;

namespace rag_2_backend.data;

public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
}