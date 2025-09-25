namespace Core.Shared;

/*
 This is a classic Maybe<T> monad implementation in C# that represents optional values, often used in DDD or functional programming.
 */

/*
 public sealed class Maybe<T>

Generic class for wrapping a value of type T.

Represents either a value (Some) or no value (None).

sealed → cannot be inherited, ensures immutability pattern.

Implements IEquatable<Maybe<T>> → allows value-based equality.
 */

public sealed class Maybe<T> : IEquatable<Maybe<T>>
{
    /*
     private readonly T _value;

Stores the actual value.

readonly ensures immutable after construction.
     */
    private readonly T _value;

    /*
     Private constructor ensures that Maybe instances are only created through factory methods like From or None.

Helps enforce controlled creation.
     */
    private Maybe(T value) => _value = value;

    /*
     HasValue → true if _value is not null.

HasNoValue → true if _value is null.

Provides a safe way to check presence of a value without throwing exceptions.
     */

    public bool HasValue => _value != null;

    public bool HasNoValue => !HasValue;

    /*
     Value property exposes the contained value.

Throws exception if accessed when no value exists, enforcing safety.
     */

    public T Value
    {
        get
        {
            if (HasNoValue || _value is null)
            {
                throw new InvalidOperationException("The value can not be accessed because it does not exist.");
            }

            return _value;
        }
    }

    /*
     None → creates a Maybe with no value (default of T).

From(T value) → creates a Maybe with a value.

Implicit conversion → allows writing:

    Maybe<int> maybe = 42; // automatically wrapped

     */

    public static Maybe<T> None => new Maybe<T>(default!);

    public static Maybe<T> From(T value) => new Maybe<T>(value);

    public static implicit operator Maybe<T>(T value) => new Maybe<T>(value);

    /*
     6. Equality

    Implements value-based equality:

Two Maybe instances are equal if:

Both have no value, or

Both have values and those values are equal.

GetHashCode returns 0 if no value, otherwise hash of value.

Ensures consistent behavior in collections like dictionaries or sets.
     */

    public bool Equals(Maybe<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (HasNoValue && other.HasNoValue)
        {
            return true;
        }

        if (HasNoValue || other.HasNoValue)
        {
            return false;
        }

        return Value!.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is T value)
        {
            return Equals(new Maybe<T>(value));
        }

        if (obj is Maybe<T> maybe)
        {
            return Equals(maybe);
        }

        return false;
    }

    /*
     1. Purpose of GetHashCode

GetHashCode is used by .NET hash-based collections, such as:

Dictionary<TKey, TValue>

HashSet<T>

ConcurrentDictionary<TKey, TValue>

It provides a numeric representation of an object that determines where it will be stored in a hash table.

Consistency rule:

If two objects are equal (Equals), their hash codes must be equal.

If objects are not equal, hash codes should ideally differ (to reduce collisions).
     */

    public override int GetHashCode() => HasNoValue ? 0 : Value!.GetHashCode();
}

/*
 Why use Maybe<T> in DDD

Represents optional values without nulls.

Avoids null reference exceptions.

Makes code self-documenting:

Works well with functional patterns, e.g., Map, Bind (though not implemented here yet).

Summary

Maybe<T> is a wrapper for optional values.

HasValue / HasNoValue → check existence.

Value → access the inner value safely (throws if absent).

None / From / implicit operator → convenient creation.

Equals / GetHashCode → proper value equality.

Perfect for DDD entities, value objects, and safe null handling.
 */
