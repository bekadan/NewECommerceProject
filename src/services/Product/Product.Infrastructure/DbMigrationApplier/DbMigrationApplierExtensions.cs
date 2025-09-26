using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Product.Infrastructure.DbMigrationApplier;

public static class DbMigrationApplierExtensions
{
    public static IApplicationBuilder UseAllDbMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Get all registered DbContexts
        var dbContextTypes = serviceProvider
            .GetServices<DbContext>()
            .Select(db => db.GetType())
            .Distinct();

        foreach (var dbContextType in dbContextTypes)
        {
            var dbContext = (DbContext)serviceProvider.GetRequiredService(dbContextType);
            dbContext.Database.EnsureDbApplied();
        }

        return app;
    }
}
