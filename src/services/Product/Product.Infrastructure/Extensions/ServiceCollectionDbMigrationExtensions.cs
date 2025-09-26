using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Product.Infrastructure.Extensions;

public static class ServiceCollectionDbMigrationExtensions
{
    /// <summary>
    /// Adds a startup task to ensure the database is created or migrated.
    /// </summary>
    public static IServiceCollection AddDatabaseMigration<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Add a hosted service that will run on startup
        services.AddHostedService<DbMigrationHostedService<TDbContext>>();
        return services;
    }
}
