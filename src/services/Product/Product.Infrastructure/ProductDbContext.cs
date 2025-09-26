using Core.Events.Abstractions;
using Core.Events.Dispatching;
using Core.Shared.Abstractions;
using Core.Shared.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Product.Infrastructure;

public class ProductDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ProductDbContext(DbContextOptions<ProductDbContext> options, IDomainEventDispatcher dispatcher)
        : base(options)
    {
        _domainEventDispatcher = dispatcher;
    }

    public DbSet<Domain.Entities.Product> Products => Set<Domain.Entities.Product>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreated();
            if (entry.State == EntityState.Modified)
                entry.Entity.SetModified();
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified; // 🚨 prevent EF from generating DELETE
                entry.Entity.MarkDeleted();          // ✅ set Deleted = true, DeletedOnUtc = now
            }
        }

        // 1. Gather domain events
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot<IDomainEvent>>()
            .SelectMany(e => e.Entity.Events)
            .ToList();

        // 2. Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // 3. Dispatch domain events
        if (domainEvents.Any())
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            foreach (var entry in ChangeTracker.Entries<AggregateRoot<IDomainEvent>>())
            {
                entry.Entity.ClearDomainEvents();
            }
        }

        return result;
    }
}
