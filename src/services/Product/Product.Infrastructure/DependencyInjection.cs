using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Infrastructure.Extensions;

namespace Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(options=>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDatabaseMigration<ProductDbContext>();

        return services;
    }
}
