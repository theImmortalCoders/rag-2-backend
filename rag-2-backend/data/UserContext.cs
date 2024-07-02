namespace rag_2_backend.data;
using models;
using Microsoft.EntityFrameworkCore;

public class UserContext:DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {}
    public DbSet<User> Users { get; set; }
}