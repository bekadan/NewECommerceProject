using Core.Exceptions.Types;
using Core.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.Exceptions.Middleware;

/*
 this code is a global exception handling middleware in ASP.NET Core. It’s a fundamental piece of building production-grade APIs 
because it ensures that all unhandled 
exceptions are caught, logged properly, and returned to the client in a consistent, structured format — without leaking internal details.
 */

/*
 * 
 * I would like to remind what middleware in asp.net core to you for a moment.
 🌐 1. What Is Middleware in ASP.NET Core?

A middleware is a piece of code that sits in the HTTP request pipeline.
Every request passes through the pipeline before reaching your controllers, and every response passes through before being sent back.

ExceptionHandlingMiddleware is a global safety net: it catches exceptions that bubble up during request handling and makes sure they’re 
dealt with gracefully instead of crashing the app or returning ugly stack traces.
 */

public class ExceptionHandlingMiddleware
{
    /*
     RequestDelegate _next: The next piece of middleware in the pipeline.

ILogger _logger: A custom logger abstraction for structured logging.

_includeDetails: Whether to include stack traces and inner exceptions in the response (e.g., false in production, true in development).
     */
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly bool _includeDetails;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger, bool includeDetails = false)
    {
        _next = next;
        _logger = logger;
        _includeDetails = includeDetails;
    }

    /*
     🔁 3. The Request Flow (InvokeAsync)

    This method is called for every request.

await _next(context) passes the request to the next middleware (e.g., MVC, routing, controllers).

If any exception is thrown during request processing, it’s caught here and passed to HandleExceptionAsync.

✅ This means no exception can escape unhandled — perfect for a stable API.
     */

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /*
     🛠️ 4. Centralized Exception Handling (HandleExceptionAsync)
     */

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        /*
         Sets the response content type to JSON.

Uses MapToStatusCode() (explained later) to translate exception types into proper HTTP status codes.

Prepares the response with that status code.
         */

        context.Response.ContentType = "application/json";

        var statusCode = MapToStatusCode(exception);
        context.Response.StatusCode = statusCode;

        /*
         Each error response includes a Correlation ID, a unique request identifier.

This allows you (or your logs) to trace a specific error across microservices or distributed systems.

If the client provided a correlation ID in headers, we use it. Otherwise, we generate one.

✅ Best practice: Always include correlation IDs in logs and responses.
         */

        // Correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

        /*
         🪵 6. Structured Logging
        Instead of logging a simple string, we log structured data — key-value pairs that logging platforms (like Serilog, ELK, or Application Insights) can parse and filter.

If _includeDetails is true, we also log the exception stack trace and inner exceptions.
         */

        // Structured logging with Serilog / Core.Logging
        var logProperties = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["StatusCode"] = statusCode,
            ["ExceptionType"] = exception.GetType().Name
        };

        if (_includeDetails)
            logProperties["ExceptionDetails"] = GetExceptionDetails(exception);

        /*
         Logging severity depends on the type of exception:
        Handled exception: Known business/domain errors (e.g., NotFoundException) → logged as Error.

Unhandled exception: Unknown or unexpected exceptions → logged as Critical.

✅ This helps you distinguish bugs (critical) from expected errors (validation, not found, etc.).
         */

        // Log as critical for unhandled, or error for AppExceptions
        if (exception is AppException)
            _logger.Error(exception, "Handled exception {@Properties}", logProperties);
        else
            _logger.Critical(exception, "Unhandled exception {@Properties}", logProperties);

        /*
         📦 7. Structured JSON Response
        The API response sent back to the client looks like this:
        {
  "error": "User not found",
  "statusCode": 404,
  "correlationId": "c1a4f9b2-1b23-44a2-bb9c-73d3a823f23b",
  "details": {
    "Message": "...",
    "StackTrace": "...",
    "inner": null
  }
}

        ✅ This makes your API responses predictable and machine-readable — crucial for frontend clients, integrations, and logging systems.
         */

        // Prepare structured response

        /*
         🔍 9. Optional: Include Exception Details (For Debugging)
        Builds a recursive object containing the exception message, stack trace, and inner exceptions.

Only included when _includeDetails = true (usually in development or internal APIs).

✅ This keeps production responses secure — clients don’t see sensitive implementation details.
         */
        var response = new
        {
            error = exception.Message,
            statusCode,
            correlationId,
            details = _includeDetails ? GetExceptionDetails(exception) : null
        };

        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        await context.Response.WriteAsync(result);
    }
    /*
     🧭 8. Exception-to-HTTP Mapping (MapToStatusCode)
    Each custom exception type is translated into the appropriate HTTP status code.

This is where the benefit of specific exceptions shines. Instead of checking strings or numbers, we simply use pattern matching.

If the exception is unknown → return 500 Internal Server Error.

✅ Result: consistent and meaningful API error codes.
     */
    private int MapToStatusCode(Exception ex)
    {
        return ex switch
        {
            DomainException => 400,
            ValidationException => 400,
            NotFoundException => 404,
            UnauthorizedException => 401,
            ForbiddenException => 403,
            InternalServerException => 500,
            ExternalServiceException => 502,
            AppException appEx => appEx.StatusCode,
            _ => 500
        };
    }

    private object GetExceptionDetails(Exception ex)
    {
        return new
        {
            ex.Message,
            ex.StackTrace,
            inner = ex.InnerException != null ? GetExceptionDetails(ex.InnerException) : null
        };
    }
}

/*
 * 🏆 Why This Middleware Is a Best Practice
 
| Feature                              | Benefit                                          |
| ------------------------------------ | ------------------------------------------------ |
| ✅ Centralized exception handling     | All errors handled in one place — no duplication |
| ✅ Type-based status codes            | Meaningful, RESTful responses                    |
| ✅ Structured logging                 | Easy to query, alert, and analyze logs           |
| ✅ Correlation IDs                    | Easier debugging and traceability                |
| ✅ Configurable error details         | Safe in production, helpful in dev               |
| ✅ Separation of handled vs unhandled | Better monitoring and alerting                   |

📊 Final Summary

This ExceptionHandlingMiddleware is an enterprise-grade global error handler. It:

Catches every unhandled exception in your API.

Maps them to meaningful HTTP status codes.

Logs them in a structured, searchable format.

Returns consistent, machine-readable error responses.

Supports correlation IDs for distributed tracing.

Can include or hide technical details based on environment.

✅ Bottom line: This pattern is standard in professional ASP.NET Core projects. 

It’s one of the most important components for stability, observability, and maintainability of a microservice or web API.

 */
