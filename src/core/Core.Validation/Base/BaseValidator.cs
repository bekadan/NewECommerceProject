using Core.Logging.Abstractions;
using Core.Shared;
using FluentValidation;

namespace Core.Validation.Base;

/*
 AbstractValidator<T> → FluentValidation base class, provides the RuleFor API and ValidateAsync.

Abstractions.IValidator<T> → Your DDD-style validator interface that returns a Result.

BaseValidator<T> is abstract, so it can’t be instantiated directly; concrete validators will inherit it.

Purpose: It acts as a shared foundation for all validators in your application.
 */

public abstract class BaseValidator<T> : AbstractValidator<T>, Abstractions.IValidator<T>
{
    /*
     _logger → Stores the injected logging service.

Constructor enforces that a non-null logger is provided.

ArgumentNullException → fail-fast if logger is missing.

Purpose: Ensures all validators can log errors consistently.
     */

    private readonly ILogger _logger;

    protected BaseValidator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /*
     This is the core validation method of your BaseValidator.

Returns a Result instead of throwing exceptions.

Takes the entity or DTO instance T to validate.

Supports CancellationToken for async operations (e.g., database checks inside rules).

Purpose: Combines FluentValidation validation with DDD-style error handling and logging.
     */

    public async Task<Result> ValidateAndLogAsync(T instance, CancellationToken cancellationToken = default)
    {
        /*
         Calls FluentValidation’s built-in ValidateAsync.

Returns a ValidationResult object containing:

IsValid → true if all rules pass.

Errors → list of ValidationFailure objects if rules fail.

Purpose: Runs the actual validation rules defined in derived classes.
         */

        var validationResult = await base.ValidateAsync(instance, cancellationToken);

        /*
         If all rules pass, return a successful Result.

Result.Success() → standard DDD pattern for operations that completed successfully.

Purpose: Early exit for valid objects.
         */

        if (validationResult.IsValid)
        {
            return Result.Success();
        }


        /*
         Groups all validation failures by the property name.

Converts to Dictionary<string, string[]>, where:

Key → property name (e.g., "Price").

Value → array of error messages for that property.

Purpose: Makes errors structured and easy to log or return.
         */

        IDictionary<string, string[]> errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        /*
         Constructs a domain-specific exception for logging purposes.

Not thrown; only passed to the logger.

Contains all property-level errors for context.

Purpose: Provides rich context to the logging system without affecting the Result.
         */

        var ex = new Exceptions.Types.ValidationException(
            "Validation failed",
            errors
        );

        /*
         Logs the failure using your logger
        
        ValidationException is only for logging

Not thrown to the caller.

Contains structured Errors dictionary for observability..


        Result.Failure is returned to the caller

Keeps functional/DDD pattern intact.

Services or API layers consume the Result without catching exceptions.


Structured logging:

{Type} → the entity type being validated.

{@Errors} → captures the errors dictionary in a structured format.

ex → optional exception for stack trace or error monitoring tools.

Purpose: Centralized logging of all validation failures for observability.
         */

        _logger.Error(
            ex,
            "Validation failed for {Type}. Errors: {@Errors}",
            typeof(T).Name,
            errors
        );

        /*
         Flattens all property errors into a single string.

Useful for returning a concise Error.Message in the Result.

Purpose: Converts structured errors into a human-readable summary for the domain layer.
         */

        var combinedMessage = string.Join("; ", errors.SelectMany(e => e.Value));

        /*
         Wraps the combined message into your DDD-style Error.

Result.Failure → indicates the operation failed due to validation.

VALIDATION_ERROR → standard code for domain validation failures.

Purpose: Returns structured failure information to the service or application layer.
         */

        return Result.Failure(Shared.Primitives.Error.Create(combinedMessage, "VALIDATION_ERROR"));
    }
}

/*
 12. How to Use It

public class ProductValidator : BaseValidator<Product>
{
    public ProductValidator(ILogger logger) : base(logger)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero");
    }
}

// Usage
var validator = new ProductValidator(logger);
Result result = await validator.ValidateAndLogAsync(product);

if (result.IsFailure)
{
    Console.WriteLine(result.Error.Message);
}

Define rules in child classes using FluentValidation’s RuleFor.

Call ValidateAndLogAsync → handles validation, logging, and returns a Result.

 */

/*
 This approach follows DDD best practices:

Validation does not throw — uses Result.

Logging is centralized and structured.

Validation rules remain reusable and decoupled.

Domain services can act on the result without exception handling overhead.
 */
