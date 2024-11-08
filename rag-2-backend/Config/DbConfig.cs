#region

using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Database;

#endregion

namespace rag_2_backend.Config;

public static class DbConfig
{
    public static void MigrateDb(WebApplication webApplication)
    {
        using var scope = webApplication.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<DatabaseContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or initializing the database.");
            throw;
        }
    }
}