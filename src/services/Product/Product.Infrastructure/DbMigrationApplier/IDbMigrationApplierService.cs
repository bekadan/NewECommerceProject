using Microsoft.EntityFrameworkCore;

namespace Product.Infrastructure.DbMigrationApplier;

public interface IDbMigrationApplierService
{
    public void Initialize();
}

public interface IDbMigrationApplierService<TDbContext> : IDbMigrationApplierService
    where TDbContext : DbContext
{ }
