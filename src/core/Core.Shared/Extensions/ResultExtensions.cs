using Core.Shared.Primitives;


namespace Core.Shared.Extensions;

/*
 
 1. Overview

These are extension methods for the Result and Result<TValue> classes.

Purpose: compose domain operations safely while handling success and failure in a fluent, functional way.

They allow:

Conditional checks (Ensure)

Mapping / transforming values (Map)

Chaining dependent operations (Bind)

Pattern matching (Match)

Side-effects without altering results (Tap)

Fits in DDD:

Makes business rules explicit

Avoids exceptions for control flow

Integrates seamlessly with async domain services and validation pipelines


 */

public static class ResultExtensions
{
    /*
     
     Ensure Methods
    
     Validates a Result<TValue> against a predicate.

If result is failure → return original failure

If predicate fails → return failure with provided Error

Otherwise → return the same success result

Async overloads allow either Task<Result<TValue>> or async predicates (Func<TValue, Task<bool>>).
     */

    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value()) ? Result.Success(result.Value()) : Result.Failure<TValue>(error);
    }

    public static async Task<Result<TValue>> Ensure<TValue>(
        this Task<Result<TValue>> resultTask, Func<TValue, bool> predicate, Error error)
    {
        Result<TValue> result = await resultTask;

        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value()) ? Result.Success(result.Value()) : Result.Failure<TValue>(error);
    }

    public static async Task<Result<TValue>> Ensure<TValue>(
        this Result<TValue> result, Func<TValue, Task<bool>> predicate, Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Value()) ? Result.Success(result.Value()) : Result.Failure<TValue>(error);
    }

    /*
     Match Methods

    Purpose:

Provides pattern matching for Result.

Executes onSuccess if successful, otherwise onFailure with the Error.

Async overloads handle Task<Result> or Task<Result<TValue>>.
     */

    public static async Task<T> Match<T>(this Task<Result> resultTask, Func<T> onSuccess, Func<Error, T> onFailure)
    {
        Result result = await resultTask;

        return result.Match(onSuccess, onFailure);
    }

    public static T Match<T>(this Result result, Func<T> onSuccess, Func<Error, T> onFailure)
        => result.IsSuccess ? onSuccess() : onFailure(result.Error);

    public static async Task<T> Match<TValue, T>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, T> onSuccess,
        Func<Error, T> onFailure)
    {
        Result<TValue> result = await resultTask;

        return result.Match(onSuccess, onFailure);
    }

    public static T Match<TValue, T>(this Result<TValue> result, Func<TValue, T> onSuccess, Func<Error, T> onFailure)
        => result.IsSuccess ? onSuccess(result.Value()) : onFailure(result.Error);

    /*
     Purpose:

Transforms the value of a successful result to another type.

Preserves failure state if original result is failure.

Async overloads available.


    Useful for converting domain objects between layers.
     */

    public static Result<T> Map<T>(this Result result, Func<Result<T>> func)
        => result.IsSuccess ? func() : Result.Failure<T>(result.Error);

    public static Result<T> Map<TValue, T>(this Result<TValue> result, Func<TValue, T> func)
        => result.IsSuccess ? Result.Success(func(result.Value())) : Result.Failure<T>(result.Error);

    public static async Task<Result<T>> Map<TValue, T>(this Task<Result<TValue>> resultTask, Func<TValue, T> func)
    {
        Result<TValue> result = await resultTask;

        return result.IsSuccess ? Result.Success(func(result.Value())) : Result.Failure<T>(result.Error);
    }

    /*
     Purpose:

Chains dependent operations (monadic flatMap).

Executes func only if result.IsSuccess

Returns failure immediately if previous result failed.

Async overloads support tasks and functions returning nullable or scalar types.

    Ensures fail-fast domain operations

Makes business pipelines explicit.
     */

    public static async Task<Result> Bind<TValue>(this Result<TValue> result, Func<TValue, Task<Result>> func)
        => result.IsSuccess ? await func(result.Value()) : Result.Failure(result.Error);

    public static async Task<Result<T>> Bind<TValue, T>(this Result<TValue> result, Func<TValue, Task<Result<T>>> func)
        => result.IsSuccess ? await func(result.Value()) : Result.Failure<T>(result.Error);

    public static async Task<Result<T>> Bind<TValue, T>(this Result<TValue> result, Func<TValue, Task<T>> func)
        where T : class
        => result.IsSuccess ? await func(result.Value()) : Result.Failure<T>(result.Error);

    /*
     Purpose:

Handles operations that might return null.

If func returns null → wraps it as a failure with the given Error.

This is common in DDD when fetching optional aggregates or entities.
     */

    public static async Task<Result<T>> Bind<TValue, T>(this Result<TValue> result, Func<TValue, Task<T?>> func, Error error)
        where T : class
    {
        if (result.IsFailure)
        {
            return Result.Failure<T>(result.Error);
        }

        T? value = await func(result.Value());

        return value is null ? Result.Failure<T>(error) : Result.Success(value);
    }

    public static async Task<Result<T>> Bind<TValue, T>(this Task<Result<TValue>> resultTask, Func<TValue, Task<T>> func)
    {
        Result<TValue> result = await resultTask;

        return result.IsSuccess ? Result.Success(await func(result.Value())) : Result.Failure<T>(result.Error);
    }

    /*
     Specialized for struct / value types

Ensures nullable semantics handled consistently

Fits for domain rules returning primitives (like int, bool, decimal)
     */

    public static async Task<Result<T>> BindScalar<TValue, T>(this Result<TValue> result, Func<TValue, Task<T>> func)
        where T : struct
    {
        return result.IsSuccess ? await func(result.Value()) : Result.Failure<T>(result.Error);
    }

    /*
     Purpose:

Performs a side-effect (like logging) if the result is successful

Returns the original result unchanged

Ensures side-effects do not alter domain outcome

    Common in DDD for observability or events.
     */

    public static async Task<Result> Tap<TValue>(this Task<Result<TValue>> resultTask, Action<TValue> action)
    {
        Result<TValue> result = await resultTask;

        if (result.IsSuccess)
        {
            action(result.Value());
        }

        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }
}

/*
 
 SUMMARY

 | Method            | Purpose                                          | Sync / Async |
| ----------------- | ------------------------------------------------ | ------------ |
| `Ensure`          | Validate result value against a predicate        | Sync & Async |
| `Match`           | Pattern match: success/failure handling          | Sync & Async |
| `Map`             | Transform success value while preserving failure | Sync & Async |
| `Bind`            | Chain dependent operations (fail-fast)           | Async        |
| `Bind` (nullable) | Chain operations returning nullable values       | Async        |
| `BindScalar`      | Chain operations returning value types           | Async        |
| `Tap`             | Execute side-effects without changing result     | Async        |

 
 */

/*
 10. How it fits in DDD

Domain Operations Composition

You can chain multiple business rules without manually checking IsSuccess each time.

Fail-fast Business Logic

Operations short-circuit on failure, propagating errors naturally.

Explicit Error Handling

Each step returns a Result with an Error Value Object → errors are first-class citizens.

Async-Friendly

Perfect for domain services, repositories, and aggregate interactions in async DDD applications.

Functional / Declarative Style

Encourages clean, readable domain pipelines:

var finalResult = await GetCustomer()
    .Bind(c => ValidateCustomer(c))
    .Bind(c => PlaceOrder(c))
    .Map(order => order.TotalPrice)
    .Tap(price => _logger.LogInfo($"Order price calculated: {price}"));

 
This is very common in modern DDD + C#.

 */
