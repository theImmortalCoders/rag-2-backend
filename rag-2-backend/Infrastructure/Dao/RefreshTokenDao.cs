#region

using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class RefreshTokenDao(DatabaseContext context)
{
    public void RemoveTokensForUser(User user)
    {
        var unusedTokens = context.RefreshTokens.Where(r => r.User.Id == user.Id).ToList();
        context.RefreshTokens.RemoveRange(unusedTokens);
        context.SaveChanges();
    }
}