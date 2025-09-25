namespace Core.Shared.Utility;

/*
 1. Overview

Ensure is a utility class providing precondition checks for method arguments and domain invariants.

Purpose: validate inputs and domain values early, throwing ArgumentException if a condition fails.

In DDD: This is used to enforce invariants in entities and value objects, ensuring that the domain state is always valid.

These methods fail fast, preventing invalid state propagation.
 */

public static class Ensure
{
    /*
     Ensures a decimal value is not zero.

Throws an exception with a custom message if violated.
     */

    /// <summary>
    /// Ensures that the specified <see cref="decimal"/> value is not zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The message to show if the check fails.</param>
    /// <param name="argumentName">The name of the argument being checked.</param>
    /// <exception cref="ArgumentException"> if the specified value is empty.</exception>
    public static void NotZero(decimal value, string message, string argumentName)
    {
        if (value == decimal.Zero)
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    /*
     Ensures a decimal value does not exceed a threshold.
     */

    /// <summary>
    /// Ensures that the specified <see cref="decimal"/> value is not greater than zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The message to show if the check fails.</param>
    /// <param name="argumentName">The name of the argument being checked.</param>
    /// <exception cref="ArgumentException"> if the specified value is empty.</exception>
    public static void NotGreaterThan(decimal value, decimal treshhold, string message, string argumentName)
    {
        if (value > treshhold)
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    /*
     Ensures a decimal value is above a minimum threshold.
    Ensures domain invariants like positive quantities.
     */

    /// <summary>
    /// Ensures that the specified <see cref="decimal"/> value is not less than zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The message to show if the check fails.</param>
    /// <param name="argumentName">The name of the argument being checked.</param>
    /// <exception cref="ArgumentException"> if the specified value is empty.</exception>
    public static void NotLessThan(decimal value, decimal min, string message, string argumentName)
    {
        if (value < min)
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    /*
     Ensures a string is not null, empty, or whitespace.
     */

    /// <summary>
    /// Ensures that the specified <see cref="string"/> value is not empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The message to show if the check fails.</param>
    /// <param name="argumentName">The name of the argument being checked.</param>
    /// <exception cref="ArgumentException"> if the specified value is empty.</exception>
    public static void NotEmpty(string value, string message, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    /*
     Ensures a Guid is not empty.
    Common in DDD for identifiers of entities or aggregates.
     */

    /// <summary>
    /// Ensures that the specified <see cref="Guid"/> value is not empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The message to show if the check fails.</param>
    /// <param name="argumentName">The name of the argument being checked.</param>
    /// <exception cref="ArgumentException"> if the specified value is empty.</exception>
    public static void NotEmpty(Guid value, string message, string argumentName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(message, argumentName);
        }
    }

    /*
     Ensures a DateTime value is initialized (not default).
     */

    /// <summary>
    /// Ensures that the specified <see cref="DateTime"/> value is not empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The message to show if the check fails.</param>
    /// <param name="argumentName">The name of the argument being checked.</param>
    /// <exception cref="ArgumentException"> if the specified value is null.</exception>
    public static void NotEmpty(DateTime value, string message, string argumentName)
    {
        if (value == default)
        {
            throw new ArgumentException(message, argumentName);
        }
    }
}


/*
 8. How it fits in DDD

Entity & Value Object Invariants

Ensures entities or value objects are always in valid state.

Example in a Value Object:

public Price(decimal amount)
{
    Ensure.NotLessThan(amount, 0, "Price cannot be negative", nameof(amount));
    Amount = amount;
}

Fail-Fast Principle

Invalid inputs are rejected immediately, before they corrupt domain state.

Readable Domain Code

Replaces repeated if checks with self-documenting utility methods.

Consistency Across Domain

Using a single Ensure class ensures uniform validation rules.
 
In short: Ensure is a fail-fast, domain validation utility, 
commonly used in DDD to protect invariants in entities and value objects, making domain code safe and readable.

 */
