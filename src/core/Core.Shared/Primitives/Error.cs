namespace Core.Shared.Primitives;

/*
 This is a domain-specific error object, 
implemented as a Value Object. In DDD, a Value Object is an immutable object that is defined by its attributes rather than an identity.
 */


/// <summary>
/// Represents a concrete domain error.
/// </summary>

/*
 sealed: cannot be inherited → ensures consistency in behavior.

Inherits from ValueObject, a base class (not shown here) that typically handles equality based on the attributes of the object.

This means two Error instances are equal if their Code and Message are equal.
 */

public sealed class Error : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>

    /*
     Assigns Code and Message.

These properties are readonly (no setters), making the object immutable.

Immutability is important in DDD because Value Objects should never change once created.
     */

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /*
     Code: a unique identifier for the type of error (e.g., "USER_NOT_FOUND").

Message: human-readable error description.

This separation allows for structured handling of errors and localization if needed.
     */

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /*
     Implicit conversion to string

    Allows you to write code like this:

    Error err = new Error("NOT_FOUND", "Item not found");
string code = err; // automatically uses err.Code

    Makes the class more convenient to use in logging, exceptions, or APIs.

     */

    public static implicit operator string(Error? error) => error?.Code ?? string.Empty;

    /*
     Required by the ValueObject base class.

Defines which properties determine equality.

Here, only Code is considered for equality (Message may vary without breaking identity).

This ensures two errors with the same Code are considered equal, even if the Message differs slightly.
     */

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }

    /// <summary>
    /// Gets the empty error instance.
    /// </summary>
    /// 
    /*
     * 
     * Represents a "no error" instance.

Useful as a default value to avoid null.
     */
    internal static Error None => new Error(string.Empty, string.Empty);

    /*
     A factory method to simplify creation of errors with a default code.

    Example?

    var error = Error.Create("Something went wrong"); // code = "ERROR"
     */
    public static Error Create(string message, string code = "ERROR")
    {
        return new Error(code, message);
    }
}

/*
 c) Why Error is a Value Object

It represents a domain error, not a distinct entity.

Immutable: once created, the error’s Code and Message never change.

Equality is based on Code (and potentially Message if needed).

Can be safely reused across the system without worrying about identity conflicts.
 */

/*
 ✅ Summary:
The Error class is a DDD Value Object representing domain-specific errors. It is:

Immutable

Comparable by value (using Code)

Convenient to use (implicit conversion, factory method)

Suitable for returning structured errors in domain operations
 */