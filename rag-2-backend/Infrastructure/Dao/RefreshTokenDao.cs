#region

using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class RefreshTokenDao(DatabaseContext context)
{
    public virtual async Task RemoveTokensForUser(User user)
    {
        var unusedTokens = await context.RefreshTokens.Where(r => r.User.Id == user.Id).ToListAsync();
        context.RefreshTokens.RemoveRange(unusedTokens);
        await context.SaveChangesAsync();
    }

    public virtual async Task RemoveTokenByToken(string token)
    {
        var unusedTokens = await context.RefreshTokens.Where(r => r.Token == token).ToListAsync();
        context.RefreshTokens.RemoveRange(unusedTokens);
        await context.SaveChangesAsync();
    }
}