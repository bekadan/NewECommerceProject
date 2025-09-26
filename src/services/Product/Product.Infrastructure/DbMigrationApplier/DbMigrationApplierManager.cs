using Microsoft.EntityFrameworkCore;

namespace Product.Infrastructure.DbMigrationApplier;

public class DbMigrationApplierManager<TDbContext> : IDbMigrationApplierService<TDbContext>
where TDbContext : DbContext
{
    private readonly TDbContext _context;

    public DbMigrationApplierManager(TDbContext context)
    {
        _context = context;
    }

    public void Initialize()
    {
        _context.Database.EnsureDbApplied();
    }
}
