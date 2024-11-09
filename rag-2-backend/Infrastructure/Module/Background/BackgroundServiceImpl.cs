#region

using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Infrastructure.Module.Background;

public class BackgroundServiceImpl(
    IServiceProvider serviceProvider
) : BackgroundService
{
    private DatabaseContext _dbContext = null!;
    private StatsUtil _statsUtil = null!;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        _statsUtil = scope.ServiceProvider.GetRequiredService<StatsUtil>();

        while (!cancellationToken.IsCancellationRequested)
        {
            DeleteUnusedAccountTokens();
            DeleteUnusedRefreshTokens();
            DeleteUnusedPasswordResetTokens();
            UpdateCachedStats();

            await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
        }
    }

    //

    private void DeleteUnusedAccountTokens()
    {
        var unconfirmedUsers = new List<Database.Entity.User>();
        var unusedTokens = _dbContext.AccountConfirmationTokens
            .Include(t => t.User)
            .Where(t => t.Expiration < DateTime.Now).ToList();

        foreach (var users in unusedTokens.Select(token =>
                     new List<Database.Entity.User>(_dbContext.Users.Where(u =>
                         u.Email == token.User.Email && !u.Confirmed))))
            unconfirmedUsers.AddRange(users);

        _dbContext.Users.RemoveRange(unconfirmedUsers);
        _dbContext.AccountConfirmationTokens.RemoveRange(unusedTokens);
        _dbContext.SaveChanges();

        Console.WriteLine("Deleted " + unconfirmedUsers.Count + " unconfirmed accounts with tokens");
    }

    private void DeleteUnusedRefreshTokens()
    {
        var unusedTokens = _dbContext.RefreshTokens.Where(b => b.Expiration < DateTime.Now).ToList();
        _dbContext.RefreshTokens.RemoveRange(unusedTokens);
        _dbContext.SaveChanges();

        Console.WriteLine("Deleted " + unusedTokens.Count + " expired refresh tokens");
    }

    private void DeleteUnusedPasswordResetTokens()
    {
        var unusedTokens = _dbContext.PasswordResetTokens.Where(b => b.Expiration < DateTime.Now).ToList();
        _dbContext.PasswordResetTokens.RemoveRange(unusedTokens);
        _dbContext.SaveChanges();

        Console.WriteLine("Deleted " + unusedTokens.Count + " expired password reset tokens");
    }

    private async void UpdateCachedStats()
    {
        var games = await _dbContext.Games.ToListAsync();
        foreach (var game in games) _statsUtil.UpdateCachedGameStats(game);

        _statsUtil.UpdateCachedStats();

        Console.WriteLine("Stats updated.");
    }
}