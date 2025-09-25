namespace Core.Shared.Extensions;

/*
 1. Overview

This class provides extension methods for the Maybe<T> type (a monad-like structure).

Maybe<T> represents a value that may or may not exist, avoiding nulls.

These extensions make it easier to compose asynchronous or synchronous operations safely without null reference exceptions.

Fits in DDD: Useful for optional domain data, validation, or chaining operations in a functional style.
 */

public static class MaybeExtensions
{
    /*
     What it does:

Filters the Maybe<T> based on a predicate.

If maybe has a value and predicate returns true → keep the value.

Otherwise → return Maybe<T>.None (represents "no value").

    Example: 
    Maybe<int> maybeNumber = new Maybe<int>(5);
var result = maybeNumber.Ensure(n => n > 10); // result = None

    Helps enforce domain rules on optional values.
     */
    public static Maybe<T> Ensure<T>(this Maybe<T> maybe, Func<T, bool> predicate) =>
        maybe.HasValue && predicate(maybe.Value) ? maybe : Maybe<T>.None;

    /*
     
     What it does:

Applies an async function to a Maybe<TIn> only if it has a value.

If no value → returns Maybe<TOut>.None immediately.

This is like the monadic flatMap in functional programming.

    Example:

    Maybe<int> maybeNumber = new Maybe<int>(5);
var result = await maybeNumber.Bind(async n => new Maybe<int>(n * 2));

    Useful for chaining async domain operations safely.

     */

    public static Task<Maybe<TOut>> Bind<TIn, TOut>(this Maybe<TIn> maybe, Func<TIn, Task<Maybe<TOut>>> func) =>
        maybe.HasValue ? func(maybe.Value) : Task.FromResult(Maybe<TOut>.None);

    /*
     What it does:

Similar to the previous Bind, but:

Input is an asynchronous Maybe<T> (Task<Maybe<TIn>>)

Function is synchronous (Func<TIn, Maybe<TOut>>)

Awaits the task, then applies the function if a value exists.

    Example: 
    Task<Maybe<int>> maybeTask = Task.FromResult(new Maybe<int>(5));
var result = await maybeTask.Bind(n => new Maybe<int>(n * 2));

    Makes it easy to mix async and sync operations with Maybe.
     */

    public static async Task<Maybe<TOut>> Bind<TIn, TOut>(this Task<Maybe<TIn>> maybeTask, Func<TIn, Maybe<TOut>> func)
    {
        Maybe<TIn> maybe = await maybeTask;

        return maybe.HasValue ? func(maybe.Value) : Maybe<TOut>.None;
    }

    /*
     What it does:

Provides a pattern matching style for Maybe (like functional languages).

If Maybe has a value → execute onHasValue

Otherwise → execute onHasNoValue

Awaits the Task<Maybe<T>> before matching

    Task<Maybe<int>> maybeTask = Task.FromResult(new Maybe<int>(5));
int result = await maybeTask.Match(
    value => value * 2,    // if value exists
    () => 0                // if no value
);
// result = 10

    Makes handling optional async values very readable and explicit.


     */

    public static async Task<TOut> Match<TIn, TOut>(this Task<Maybe<TIn>> maybeTask, Func<TIn, TOut> onHasValue, Func<TOut> onHasNoValue)
    {
        Maybe<TIn> maybe = await maybeTask;

        return maybe.HasValue ? onHasValue(maybe.Value) : onHasNoValue();
    }
}

/*
 6. How this fits in DDD

Avoids nulls

In DDD, Maybe<T> represents optional domain data, avoiding null reference exceptions.

Encourages safe chaining

Using Bind and Ensure, you can compose operations without checking nulls repeatedly.

Supports async operations

Many domain operations are async (repository calls, domain services).

These extensions allow safe async composition with Maybe<T>.

Expressive business logic

Match makes domain intent explicit: handle both success/value and absence.
 */

/*
 ✅ In short:

These extensions make Maybe<T> powerful for domain logic:

Safe optional values

Async/sync operation chaining

Explicit business rules handling

Fits nicely in DDD, functional style, and clean architecture
 */
