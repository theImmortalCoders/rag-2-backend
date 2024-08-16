using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.Services;

public class BackgroundServiceImpl(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            CheckUnusedAccountTokens();

            await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
        }
    }

    private void CheckUnusedAccountTokens()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var unconfirmedUsers = new List<User>();
        var unusedTokens = dbContext.AccountConfirmationTokens
            .Include(t=>t.User)
            .Where(t=>t.Expiration < DateTime.Now).ToList();

        foreach (var users in unusedTokens.Select(token => new List<User>(dbContext.Users.Where(u => u.Email == token.User.Email && !u.Confirmed))))
            unconfirmedUsers.AddRange(users);

        dbContext.Users.RemoveRange(unconfirmedUsers);
        dbContext.AccountConfirmationTokens.RemoveRange(unusedTokens);
        dbContext.SaveChanges();

        Console.WriteLine("Deleted " + unconfirmedUsers.Count + " unconfirmed accounts");
    }
}