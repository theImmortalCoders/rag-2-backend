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
            await DeleteUnusedAccountTokens();
            await DeleteUnusedRefreshTokens();
            await DeleteUnusedPasswordResetTokens();
            await UpdateCachedStats();

            await Task.Delay(TimeSpan.FromHours(3), cancellationToken);
        }
    }

    //

    private async Task DeleteUnusedAccountTokens()
    {
        var unconfirmedUsers = new List<Database.Entity.User>();
        var unusedTokens = await _dbContext.AccountConfirmationTokens
            .Include(t => t.User)
            .Where(t => t.Expiration < DateTime.Now).ToListAsync();

        foreach (var users in unusedTokens.Select(token =>
                     new List<Database.Entity.User>(_dbContext.Users.Where(u =>
                         u.Email == token.User.Email && !u.Confirmed))))
            unconfirmedUsers.AddRange(users);

        _dbContext.Users.RemoveRange(unconfirmedUsers);
        _dbContext.AccountConfirmationTokens.RemoveRange(unusedTokens);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine("Deleted " + unconfirmedUsers.Count + " unconfirmed accounts with tokens");
    }

    private async Task DeleteUnusedRefreshTokens()
    {
        var unusedTokens = _dbContext.RefreshTokens.Where(b => b.Expiration < DateTime.Now).ToList();
        _dbContext.RefreshTokens.RemoveRange(unusedTokens);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine("Deleted " + unusedTokens.Count + " expired refresh tokens");
    }

    private async Task DeleteUnusedPasswordResetTokens()
    {
        var unusedTokens = _dbContext.PasswordResetTokens.Where(b => b.Expiration < DateTime.Now).ToList();
        _dbContext.PasswordResetTokens.RemoveRange(unusedTokens);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine("Deleted " + unusedTokens.Count + " expired password reset tokens");
    }

    private async Task UpdateCachedStats()
    {
        var games = await _dbContext.Games.ToListAsync();
        foreach (var game in games) await _statsUtil.UpdateCachedGameStats(game);

        await _statsUtil.UpdateCachedStats();

        Console.WriteLine("Stats updated.");
    }
}