using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.Services;

public class BackgroundServiceImpl(IServiceProvider serviceProvider) : BackgroundService
{
    private DatabaseContext _dbContext;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        while (!cancellationToken.IsCancellationRequested)
        {
            DeleteUnusedAccountTokens();
            DeleteUnusedBlacklistedJwts();
            DeleteUnusedPasswordResetTokens();

            await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
        }
    }

    private void DeleteUnusedAccountTokens()
    {
        var unconfirmedUsers = new List<User>();
        var unusedTokens = _dbContext.AccountConfirmationTokens
            .Include(t => t.User)
            .Where(t => t.Expiration < DateTime.Now).ToList();

        foreach (var users in unusedTokens.Select(token =>
                     new List<User>(_dbContext.Users.Where(u => u.Email == token.User.Email && !u.Confirmed))))
            unconfirmedUsers.AddRange(users);

        _dbContext.Users.RemoveRange(unconfirmedUsers);
        _dbContext.AccountConfirmationTokens.RemoveRange(unusedTokens);
        _dbContext.SaveChanges();

        Console.WriteLine("Deleted " + unconfirmedUsers.Count + " unconfirmed accounts");
    }

    private void DeleteUnusedBlacklistedJwts()
    {
        var unusedTokens = _dbContext.BlacklistedJwts.Where(b => b.Expiration < DateTime.Now).ToList();
        _dbContext.BlacklistedJwts.RemoveRange(unusedTokens);
        _dbContext.SaveChanges();

        Console.WriteLine("Deleted" + unusedTokens.Count + " blacklisted jwts");
    }

    private void DeleteUnusedPasswordResetTokens()
    {
        var unusedTokens = _dbContext.PasswordResetTokens.Where(b => b.Expiration < DateTime.Now).ToList();
        _dbContext.PasswordResetTokens.RemoveRange(unusedTokens);
        _dbContext.SaveChanges();

        Console.WriteLine("Deleted" + unusedTokens.Count + " password reset tokens");
    }
}