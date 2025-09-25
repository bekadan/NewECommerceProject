namespace Core.Exceptions.Types;

/*
 this AppException class — it’s a fundamental part of a clean error handling and exception strategy in enterprise-grade applications.
 */

/*
 Purpose of AppException

AppException is a base class for all custom exceptions in your application.
Its purpose is to:

✅ Provide a standardized structure for exceptions across the system.

✅ Include HTTP status codes so exceptions map easily to API responses.

✅ Allow you to build domain-specific or application-specific exceptions by inheriting from it.

✅ Centralize error handling logic and make global exception handling middleware simpler.

In other words, it’s the foundation of a consistent error-handling system.
 */

/*
 abstract means you cannot create an instance of AppException directly.

Instead, it’s meant to be inherited by more specific exceptions (like NotFoundException, ValidationException, etc.).

It inherits from the built-in .NET Exception class, so it behaves like a normal exception but with additional features.

✅ Why this matters:

By forcing inheritance, you ensure all custom exceptions share common properties and structure.
 
 */

public abstract class AppException : Exception
{
    /*
     Adds an HTTP Status Code property to the exception.

This is not present in the base Exception class — we add it because most web APIs return status codes in responses.

It helps map exceptions directly to HTTP responses in middleware.
     */
    public int StatusCode { get; }

    /*
     protected → Only child classes can call this, not external code.

Takes a message and an optional statusCode (default = 500 – internal server error).

Passes the message to the base Exception class.

Sets the StatusCode property.
     */

    protected AppException(string message, int statusCode = 500)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /*
     Same as above but also accepts an innerException.

innerException is used for exception chaining — you can wrap a lower-level exception inside a higher-level one without losing the original error details.
     */
    protected AppException(string message, Exception innerException, int statusCode = 500)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

/*
 Why Use a Base AppException?

| ✅ Feature                   | 📌 Benefit                                                                      |
| --------------------------- | ------------------------------------------------------------------------------- |
| 🧱 **Consistent structure** | All exceptions share a `StatusCode` and `Message`.                              |
| 🔄 **Centralized handling** | Your global exception middleware can handle all `AppException` types uniformly. |
| 📡 **Better API responses** | Map exceptions directly to HTTP status codes and response bodies.               |
| 🔍 **Improved logging**     | Custom exception types give more context to logs.                               |
| 🧪 **Easier testing**       | Assertions in unit tests can check for specific exception types.                |


SUMMARY

| 🧱 Feature                      | 📌 Explanation                                              |
| ------------------------------- | ----------------------------------------------------------- |
| **Base for all exceptions**     | Standardizes how errors are represented in your app.        |
| **Includes `StatusCode`**       | Makes mapping exceptions to HTTP responses straightforward. |
| **Supports exception chaining** | Helps preserve root causes while adding business context.   |
| **Promotes clean architecture** | Keeps error handling consistent and centralized.            |

AppException is the foundation of a clean error-handling system.
It turns exceptions from being random runtime errors into first-class citizens of your application’s architecture — structured, meaningful, and directly tied to API responses.

With it, your code becomes easier to debug, maintain, and scale. 💡


At first glance, it seems simpler to just have a single exception class (like AppException) and throw it with a message and status code.
But in real-world enterprise projects, creating specific exception types like NotFoundException, ValidationException, etc. is considered a best practice for several powerful reasons.

1. The Problem With “One Exception for Everything

Imagine this;

throw new AppException("User not found", 404);
throw new AppException("Invalid email address", 400);
throw new AppException("Unauthorized access", 401);

Yes, it works, but:

❌ All exceptions are the same type — your system can’t easily tell them apart.

❌ Error handling middleware has to rely on magic numbers (status codes) or string matching.

❌ Logging, monitoring, and alerting become less meaningful.

❌ Domain/business logic becomes less readable and harder to maintain.

This is fine for small toy projects, but real-world systems need more structure.


2. Why Use Multiple Specific Exceptions (Best Practices)

1. 📦 Semantic Meaning (Clarity & Self-Documentation)

Custom exception names communicate intent clearly.
Compare these two examples:

❌ Hard to understand:

throw new AppException("User not found", 404);

Crystal clear:
throw new NotFoundException("User not found");


The second one immediately tells any developer, reviewer, or log reader what type of problem occurred — without needing to read the message or status code.

2. 🔄 Better Error Handling (Type-Specific Catching)

In complex apps, you often want to handle different errors differently.
If they’re all the same type, you have to write ugly if-else conditions:

❌ Hard and error-prone:

catch (AppException ex)
{
    if (ex.StatusCode == 404) /* do one thing 
    else if (ex.StatusCode == 400) { /* do something else }
}


Elegant and robust:

catch (NotFoundException ex) { // handle resource not found  }
catch (ValidationException ex) { // handle bad input  }

This is a core principle of object-oriented design: use types to represent meaning, not just values.


3. 📊 Better Logging, Monitoring, and Alerts

Logging tools (like Serilog, Application Insights, ELK, etc.) can group and track exceptions by type.
If you only use AppException, all errors will be bucketed together.

✅ With custom types:

You can track how many ValidationExceptions occur per hour.

Alert when InternalServerException spikes (indicating a backend issue).

Quickly see which exceptions are most common and prioritize fixes.

This is invaluable in production.

4. 🧪 More Powerful Testing

Custom exceptions make unit/integration tests more expressive and reliable.

❌ With a generic exception:

await Assert.ThrowsAsync<AppException>(() => service.DoSomething());

You only know something went wrong — not what went wrong.

✅ With specific exceptions:

await Assert.ThrowsAsync<NotFoundException>(() => service.GetUser("unknown-id"));

Now your tests verify exactly the expected failure scenario.

5. 📏 Aligns With Domain-Driven Design (DDD) Principles

In DDD, exceptions are part of your ubiquitous language — they describe domain concepts.
A ValidationException or UnauthorizedException isn’t just an error — it’s a domain event: it communicates a specific rule violation.

This makes your code more expressive and aligned with business logic.

6. 🛡️ Clear Separation of Responsibility

Different exception types often belong to different layers:

ValidationException → Application / Domain Layer

UnauthorizedException → Infrastructure / Security Layer

InternalServerException → Infrastructure / System Layer

NotFoundException → Application / Repository Layer

This separation helps maintain clean architecture boundaries and makes it obvious where the error originated.

Quick Comparison: One vs Many Exceptions

| Feature                 | ✅ Multiple Custom Exceptions                   | ❌ Single Exception             |
| ----------------------- | ---------------------------------------------- | ------------------------------ |
| **Readability**         | ✅ Very clear (`throw new NotFoundException()`) | ❌ Needs message/code parsing   |
| **Error Handling**      | ✅ Type-based `catch` blocks                    | ❌ Must inspect properties      |
| **Logging & Analytics** | ✅ Grouped by type                              | ❌ All look the same            |
| **Testing**             | ✅ Can assert specific exception types          | ❌ Only generic exception check |
| **Maintainability**     | ✅ Easier to evolve and refactor                | ❌ Grows messy over time        |
| **Domain Expression**   | ✅ Matches domain concepts                      | ❌ Loses semantic meaning       |

4. Best Practice Summary

✅ Always create specific exceptions for distinct error cases — especially those that map to HTTP status codes or domain rules.
✅ Derive them from a shared base (AppException) to keep a consistent structure (e.g., status code, message).
✅ Keep names descriptive (NotFoundException, ValidationException, UnauthorizedException, etc.).
✅ Group them logically by responsibility or layer.

Golden Rule

“Exceptions are part of your API. Make them as meaningful, typed, and descriptive as any other part of your code.”


✅ TL;DR

We don’t create NotFoundException, ValidationException, etc. just to be fancy —
we do it because:

They express intent clearly.

They make error handling cleaner.

They improve logging, testing, and monitoring.

They align with clean architecture and DDD principles.

In a small toy project, one exception might be fine.
But in a real-world production system, specific exceptions are a must-have. 🚀


 */

//Let's create other type of Exceptions