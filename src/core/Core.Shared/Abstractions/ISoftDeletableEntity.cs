namespace Core.Shared.Abstractions;

/// <summary>
/// Represents the marker interface for soft-deletable entities.
/// </summary>
/// 

/*
 A marker interface for entities that support soft deletion.

In DDD, this allows entities to be logically deleted without actually removing them from the database, preserving history and consistency.
 */

/*
 DateTime? DeletedOnUtc { get; }

Nullable DateTime? because the entity may not be deleted yet.

Represents when the entity was soft-deleted, in UTC for consistency.

Helps track deletion history or perform audit/logging.
 */

/*
 bool Deleted { get; }

A flag indicating whether the entity is currently considered deleted.

Often used in queries to filter out deleted records:

var activeProducts = products.Where(p => !p.Deleted);

 */

/*
 Why soft deletion is used in DDD

Preserve data integrity: Avoids breaking references in related entities.

Auditability: You can see when something was deleted.

Reversibility: Allows “undelete” operations if needed.

Works with EF Core global query filters:

modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.Deleted);
 */

public interface ISoftDeletableEntity
{
    /// <summary>
    /// Gets the date and time in UTC format the entity was deleted on.
    /// </summary>
    DateTime? DeletedOnUtc { get; }

    /// <summary>
    /// Gets a value indicating whether the entity has been deleted.
    /// </summary>
    bool Deleted { get; }
}

/*
 Summary

ISoftDeletableEntity is a cross-cutting domain abstraction for entities that support soft deletion.

DeletedOnUtc → when the entity was deleted (nullable).

Deleted → whether the entity is currently deleted.

Helps enforce consistency, auditability, and reversible deletions in DDD applications.
 */