using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Product.Infrastructure.DbMigrationApplier;

namespace Product.Infrastructure.Extensions;

internal class DbMigrationHostedService<TDbContext> : IHostedService
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public DbMigrationHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await db.Database.EnsureDbAppliedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
