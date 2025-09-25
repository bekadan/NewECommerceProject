namespace Core.Shared.Primitives;

/*
 An Entity is a domain object that is primarily defined by its identity, not its attributes.

Examples:

Customer with CustomerId

Order with OrderId

Product with ProductId

Even if all other properties are identical, two entities with different Ids are not the same.


b) Key characteristics

Identity

Every entity has a unique identifier (like Guid).

Identity persists even if properties change.

Mutable state

Entities can change properties over time (Name, Price, etc.).

Identity remains constant.

Lifecycle

Entities have a lifecycle: creation → updates → deletion.

Equality by identity

In DDD, equality is Id-based, not property-based.

That’s why Equals() and GetHashCode() are overridden here.

c) Best practices

Use GUIDs or strongly-typed IDs for entity identity.

Always implement equality operators if you want to compare entities.

Don’t include business logic in the base Entity; keep it in domain-specific classes.

Use AggregateRoot (like you have in another snippet) to group related entities.


 */

/*
 This is a base class for all entities in a DDD-based system. 
In DDD, an entity is an object with a unique identity that persists over time, independent of its attributes.
 */



public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    /// 
    /*
     Every entity has a unique identifier (Guid here).

protected set allows subclasses to set the Id internally, but not from the outside.

This Id is how equality and identity are determined.
     */
    public Guid Id { get; protected set; }

    /*
     * Equality operators
     * 
     == and != are overridden to compare entities by identity, not by memory reference or all properties.

Handles null checks gracefully:

Both null → true

One null → false

Otherwise → delegate to Equals.
     */

    public static bool operator ==(Entity a, Entity b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b) => !(a == b);

    /*
 IEquatable<Entity> implementation

    Two entities are considered equal if:

They are the same reference (memory object).

They have the same Id.

Null-check ensures safety.
 */

    /// <inheritdoc />
    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || Id == other.Id;
    }

    /*
     Handles comparisons with any object, not just Entity.

Ensures type safety: only entities of the same derived type are considered equal.

Prevents comparing entities that don’t have an assigned Id (Guid.Empty).
     */

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        if (!(obj is Entity other))
        {
            return false;
        }

        if (Id == Guid.Empty || other.Id == Guid.Empty)
        {
            return false;
        }

        return Id == other.Id;
    }

    /*
     The hash code is based on the Id.

Multiplied by a prime number (41) to reduce hash collisions.

Critical for using entities in hash-based collections like Dictionary or HashSet.
     */

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode() * 41;
}

/*
 ✅ Summary:
This Entity base class is a DDD staple. It ensures:

Identity-based equality

Safe comparisons

Correct hash codes for collections

Standardized pattern for all domain entities
 */