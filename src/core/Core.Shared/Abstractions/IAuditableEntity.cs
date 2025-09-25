namespace Core.Shared.Abstractions;

/*
IAuditableEntity is a cross-cutting abstraction for entities that need to track creation and modification times.

CreatedOnUtc → when the entity was created (always exists).

ModifiedOnUtc → when the entity was last modified (optional, may be null).

Ensures standardization and easy auditing across your domain model. 

 */

public interface IAuditableEntity
{
    /// <summary>
    /// Gets the created on date and time in UTC format.
    /// </summary>
    DateTime CreatedOnUtc { get; }

    /// <summary>
    /// Gets the modified on date and time in UTC format.
    /// </summary>
    DateTime? ModifiedOnUtc { get; }
}
