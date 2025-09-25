namespace Core.Shared.Primitives;

/*
 a) Definition

A Value Object (VO) is an object that is defined by its properties, not by a unique identifier.

Examples:

Money with Amount and Currency

Address with Street, City, ZipCode

Error with Code and Message

Two Value Objects with the same values are considered equal, unlike entities.

b) Key characteristics

Immutable

Once created, the state cannot change.

Prevents accidental side effects.

Equality by value

Equality is determined by the content of the attributes, not by identity.

No identity

No unique Id like entities.

The value itself is the identity.

Self-contained behavior

Can include methods for validation, formatting, or business logic.

d) How this fits in a DDD system

Often returned from domain operations or commands.

Can be used in validation results, e.g., a Result<T> object which will create later on may contain:

public class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
}

Makes errors first-class citizens in the domain.

 */

/*
 This is a base class for all Value Objects in DDD. It provides:

Equality comparison by value (not reference)

Hash code generation consistent with equality

A pattern (GetAtomicValues) to define which properties make up the value object
 */

public abstract class ValueObject : IEquatable<ValueObject>
{
    /*
     What it does:

Allows using == and != directly on Value Objects.

Null-safe:

Both null → true

One null → false

Otherwise, delegates to the Equals() method.

    Example:

    Money a = new Money(10, "USD");
Money b = new Money(10, "USD");
bool equal = a == b; // true

     */

    public static bool operator ==(ValueObject a, ValueObject b)
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

    public static bool operator !=(ValueObject a, ValueObject b) => !(a == b);

    /*
     What it does:

Implements IEquatable<ValueObject> for type-safe equality.

Compares two Value Objects by their values, not by reference.

Uses GetAtomicValues() to get all properties that define the object.

SequenceEqual:

Ensures that every atomic value (e.g., Amount, Currency) is equal in order.
    
     Example:

    Money a = new Money(10, "USD");
Money b = new Money(10, "USD");
bool equal = a.Equals(b); // true
     */

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    /*
     What it does:

Overrides base object.Equals to allow comparison with any object.

Steps:

Null check

Type check (GetType() ensures exact type match, not inheritance)

Cast to ValueObject

Compare atomic values using SequenceEqual

This ensures consistent equality behavior whether you use ==, Equals(ValueObject), or Equals(object).
     */

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (GetType() != obj.GetType())
        {
            return false;
        }

        if (!(obj is ValueObject valueObject))
        {
            return false;
        }

        return GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());
    }

    /*
     What it does:

Computes a hash code based on the atomic values of the object.

Ensures that equal value objects have the same hash code (critical for dictionaries or sets).

Uses System.HashCode (built-in, optimized for combining multiple values).

    Example: 

    var a = new Money(10, "USD");
var b = new Money(10, "USD");

HashSet<Money> set = new();
set.Add(a);
bool contains = set.Contains(b); // true
     */

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hashCode = default;

        foreach (object obj in GetAtomicValues())
        {
            hashCode.Add(obj);
        }

        return hashCode.ToHashCode();
    }

    /*
     What it does:

Abstract method → every Value Object must implement it.

Defines the set of properties that determine equality.

Used by Equals() and GetHashCode().

    Example:

    public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}
    Here, Amount and Currency define the value object.

Two Money objects are equal iff both Amount and Currency match.


     */

    /// <summary>
    /// Gets the atomic values of the value object.
    /// </summary>
    /// <returns>The collection of objects representing the value object values.</returns>
    protected abstract IEnumerable<object> GetAtomicValues();
}

/*
 SUMMARY

| Feature                                    | How it works                                        |
| ------------------------------------------ | --------------------------------------------------- |
| Equality operators (`==`, `!=`)            | Null-safe, delegate to `Equals()`                   |
| Type-safe equality (`Equals(ValueObject)`) | Compares atomic values                              |
| General equality (`Equals(object)`)        | Safe for any object, type must match exactly        |
| Hash code (`GetHashCode()`)                | Combines atomic values, consistent with equality    |
| Atomic values (`GetAtomicValues`)          | Abstract; derived classes define what values matter |

 */

/*
 Key DDD Concept:

Value Objects are immutable and compared by value, not identity.

This base class enforces that pattern consistently across all value objects in your domain.
 */
