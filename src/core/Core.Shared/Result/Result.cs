using Core.Shared.Primitives;

namespace Core.Shared;

/*
 This code implements a Result pattern, also sometimes called a Functional Result type.

Purpose: Represent the outcome of an operation without using exceptions for control flow.

Features:

Encapsulates success/failure state (IsSuccess / IsFailure)

Carries an error object (Error) if the operation failed

Can optionally carry a value (TValue) when successful

Fits in DDD:

Often used in domain operations, commands, or services to return operation outcomes in a structured way.
 */



public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with the specified parameters.
    /// </summary>
    /// <param name="isSuccess">The flag indicating if the result is successful.</param>
    /// <param name="error">The error.</param>
    /// 

    /*
 2. Result (non-generic)

Protected → can’t instantiate directly, only via factory methods (Success / Failure)

Enforces consistency:

Success cannot have an error

Failure must have an error
 */
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException();
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /*
     IsSuccess: true if operation succeeded

IsFailure: convenience property (!IsSuccess)

Error: contains domain-specific error (a ValueObject from previous discussion)
     */

    /// <summary>
    /// Gets a value indicating whether the result is a success result.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure result.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Returns a success <see cref="Result"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Result"/> with the success flag set.</returns>
    /// 

    /*
     Success (no value):
     */
    public static Result Success() => new Result(true, Error.None);

    /// <summary>
    /// Returns a success <see cref="Result{TValue}"/> with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="value">The result value.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> with the success flag set.</returns>
    /// 

    /*
     Success (with value):
     */
    public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value, true, Error.None);

    

    /// <summary>
    /// Returns a failure <see cref="Result"/> with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A new instance of <see cref="Result"/> with the specified error and failure flag set.</returns>
    /// 

    /*
     Failure:
     */
    public static Result Failure(Error error) => new Result(false, error);

    /// <summary>
    /// Returns a failure <see cref="Result{TValue}"/> with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> with the specified error and failure flag set.</returns>
    /// <remarks>
    /// We're purposefully ignoring the nullable assignment here because the API will never allow it to be accessed.
    /// The value is accessed through a method that will throw an exception if the result is a failure result.
    /// </remarks>
    public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(default!, false, error);

    /// <summary>
    /// Creates a new <see cref="Result{TValue}"/> with the specified nullable value and the specified error.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="value">The result value.</param>
    /// <param name="error">The error in case the value is null.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> with the specified value or an error.</returns>
    /// 
    /*
     Create with nullable value:
    Ensures null values are treated as failures, avoiding NullReferenceException in domain logic.
     */
    public static Result<TValue> Create<TValue>(TValue? value, Error error)
        where TValue : class
        => value ?? Failure<TValue>(error);

    /// <summary>
    /// Returns the first failure from the specified <paramref name="results"/>.
    /// If there is no failure, a success is returned.
    /// </summary>
    /// <param name="results">The results array.</param>
    /// <returns>
    /// The first failure from the specified <paramref name="results"/> array,or a success it does not exist.
    /// </returns>
    /// 
    /*
     Utility Method

    Accepts an array of results

Returns first failure if any exist

Returns success otherwise

Useful when multiple domain rules need to be validated, e.g., ValidateName(), ValidateStock(), etc.
     */
    public static Result FirstFailureOrSuccess(params Result[] results)
    {
        foreach (Result result in results)
        {
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Success();
    }
}


/*
 3. Result<TValue> (generic)
Purpose

Represents operations that return a value if successful.

Inherits Result and adds strongly-typed value storage.
 */

/// <summary>
/// Represents the result of some operation, with status information and possibly a value and an error.
/// </summary>
/// <typeparam name="TValue">The result value type.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue _value;

   
    /*
     Protected internal → controlled instantiation

_value stores the successful result*/

    protected internal Result(TValue value, bool isSuccess, Error error)
        : base(isSuccess, error)
        => _value = value;

    /*
     * Value Access
     * 
     Returns the stored value only if the result is successful

Throws an exception if accessed on a failure result

Ensures domain safety: you can’t accidentally use a failed value.
     */

    public TValue Value()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException();
        }

        return _value;
    }

    /*
     Implicit Conversion
    Allows assignment of a value directly to Result<TValue>:
     */
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}

/*
 4. How this fits in DDD
a) Domain operations

DDD domain methods (entities, aggregates, domain services) often return Result<T> instead of throwing exceptions for expected errors.

Example: 

public Result<Order> PlaceOrder(Customer customer, Product product)
{
    if (!customer.IsActive)
        return Result.Failure<Order>(Error.Create("Customer is inactive", "CUSTOMER_INACTIVE"));

    var order = new Order(customer, product);
    return Result.Success(order);
}

Makes business rules explicit

Avoids hidden exception-based flows

 */

/*
 b) Composability

FirstFailureOrSuccess allows combining multiple results:

var result = Result.FirstFailureOrSuccess(
    ValidateStock(product),
    ValidateCustomer(customer),
    ValidatePayment(payment)
);

Fail fast on first rule violation

Keep domain logic clean and declarative
 */

/*
 c) Error handling

Uses Error (a Value Object) for structured errors, not strings

Makes errors part of the ubiquitous language in DDD

Improves clarity, localization, logging, and testing
 */

/*
 d) Immutability and Safety

Result and Result<T> are immutable once created

Prevents accidental modification of operation outcomes

Safe to pass around layers: Domain → Application → API
 */

/*
 SUMMARY

| Feature                   | Description                                                                                                                  |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `IsSuccess` / `IsFailure` | Indicates success or failure                                                                                                 |
| `Error`                   | Holds structured domain error (Value Object)                                                                                 |
| `Success()` / `Failure()` | Factory methods for consistent creation                                                                                      |
| `Result<TValue>`          | Extends Result with a strongly-typed value                                                                                   |
| `Value()`                 | Returns value only if success, otherwise throws                                                                              |
| `FirstFailureOrSuccess`   | Combines multiple results, returns first failure                                                                             |
| DDD Fit                   | Encapsulates **domain operation outcomes** explicitly, avoids exception abuse, integrates with business rules and validation |

 */
