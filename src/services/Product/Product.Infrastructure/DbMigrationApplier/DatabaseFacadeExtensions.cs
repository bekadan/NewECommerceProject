using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Product.Infrastructure.DbMigrationApplier;


public static class DatabaseFacadeExtensions
{
    public static async Task EnsureDbAppliedAsync(this DatabaseFacade database, CancellationToken cancellationToken = default)
    {
        if (!database.CanConnect())
            return;

        if (database.IsInMemory())
        {
            _ = database.EnsureCreated();
        }
        else if (database.IsRelational())
        {
            await database.MigrateAsync(cancellationToken);
        }
    }
}

