using Core.Shared;

namespace Core.Validation.Abstractions;

/*
 Key points:

IValidator<T> is generic, meaning it can validate any type T, such as entities, value objects, or DTOs.

ValidateAsync returns a Task<Result>:

Uses the Result pattern we discussed earlier.

Encapsulates success/failure and possible errors instead of throwing exceptions.

Asynchronous (Task) support allows async validations, like checking existence in a database or calling an external service.
 */

/*
 Parameters:

T instance → the object to validate (entity, value object, command, etc.)

CancellationToken cancellationToken → allows cancelling long-running validations

Return Type: Task<Result>

Success: all validation rules passed → Result.Success()

Failure: any rule failed → Result.Failure(Error) or Result.Failure<IEnumerable<Error>>
 */

public interface IValidator<T>
{
    /*
     Encapsulates all domain validation rules in one place.

Returns a structured result that can be processed by application services.
     */
    Task<Result> ValidateAndLogAsync(T instance, CancellationToken cancellationToken = default);
}

/*
 3. Why use this in DDD

Separation of Concerns

Validation logic is separate from entities.

Entities stay focused on state and behavior, while validators enforce rules.

Composability

Validators can be composed or reused across multiple services or aggregates.

Supports Asynchronous Checks

Can validate things like:

Database uniqueness

External service constraints

Remote API validations

Error Handling

Returns Result with Error objects → structured domain errors.

Fits nicely with functional pipelines like Bind, Map, and Ensure.
 */

/*
 4. Typical Usage Pattern

Result result = await _validator.ValidateAsync(product);

if (result.IsFailure)
{
    // handle validation errors, e.g., return to API client
    return result;
}

// proceed with domain operation

Clean fail-fast domain logic.

Integrates perfectly with Result/Maybe pipelines in domain services.

 */

/*
 SUMMARY 

| Feature         | Description                                                                                |
| --------------- | ------------------------------------------------------------------------------------------ |
| `IValidator<T>` | Generic validator interface                                                                |
| `ValidateAsync` | Validates instance of type `T` asynchronously                                              |
| Return type     | `Result` → success/failure with structured `Error`                                         |
| DDD benefit     | Keeps domain entities clean, separates rules, supports async and structured error handling |

 */
