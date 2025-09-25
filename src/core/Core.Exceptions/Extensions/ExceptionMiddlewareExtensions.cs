using Core.Exceptions.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Core.Exceptions.Extensions;

/*
 it’s an important piece of the exception handling system we just discussed. 

It’s an extension method that makes registering your global exception middleware cleaner and more reusable.
 */

/*
 🧱 1. Purpose of This File

File: ExceptionMiddlewareExtensions.cs
Namespace: Core.Exceptions.Extensions

It defines an extension method on IApplicationBuilder (the ASP.NET Core application pipeline builder).
This lets you register your custom ExceptionHandlingMiddleware using a single, clean method instead of writing boilerplate code every time.

 */

public static class ExceptionMiddlewareExtensions
{
    /*
     🛠️ UseCoreExceptionHandling(this IApplicationBuilder app, bool includeDetails = false)

Extension method:

The this IApplicationBuilder app part means this method “extends” IApplicationBuilder, which is the type returned by WebApplication or IApplicationBuilder in Program.cs.

Parameter includeDetails:

Default is false.

You can pass true when you want to include stack traces and inner exception details in the response (e.g., in a development environment).

📌 This method wraps the middleware registration logic and exposes it with a nice, fluent syntax.
     */

    public static IApplicationBuilder UseCoreExceptionHandling(this IApplicationBuilder app, bool includeDetails = false)
    {
        /*
         🪝 app.UseMiddleware<ExceptionHandlingMiddleware>(includeDetails);

This is the real work: it tells ASP.NET Core to use our custom middleware in the request pipeline.

UseMiddleware<T>() is the built-in way to add middleware.

includeDetails is passed to the constructor of ExceptionHandlingMiddleware.

✅ Result: The global exception handler is now part of the pipeline and will catch any exceptions thrown by later middleware or controllers.
         */

        return app.UseMiddleware<ExceptionHandlingMiddleware>(includeDetails);
    }
}

/*
 💡 3. How It’s Used in Program.cs

Without this extension, you’d write:

app.UseMiddleware<ExceptionHandlingMiddleware>(true);

But with this extension, you write:

app.UseCoreExceptionHandling(includeDetails: app.Environment.IsDevelopment());

✅ Cleaner, more readable.

✅ Easier to reuse across many services or projects.

✅ Consistent naming.
 */

/*
 * 
 * 🏆 Best Practices
 | Practice                               | Benefit                                                         |
| -------------------------------------- | --------------------------------------------------------------- |
| ✅ Use extension methods for middleware | Makes `Program.cs` clean and fluent                             |
| ✅ Environment-based configuration      | Hide stack traces in production, show them in dev               |
| ✅ Reusability                          | Can reuse `UseCoreExceptionHandling()` across multiple projects |
| ✅ Encapsulation                        | Keeps your startup configuration free of implementation details |

 */

/*
 📊 Final Summary

ExceptionMiddlewareExtensions is a small but powerful utility that improves how we register our global exception handling middleware.

It provides a clean, reusable, and conventional way to enable exception handling.

It keeps Program.cs more readable and maintainable.

It lets you easily toggle developer-friendly exception details.

In short, this is a best-practice pattern in ASP.NET Core: always create extension methods for custom middleware instead of calling UseMiddleware<>() directly.
 */

/*
 ✅ Example Usage:

var app = builder.Build();

// Add global error handling
app.UseCoreExceptionHandling(includeDetails: app.Environment.IsDevelopment());

Now your exception handling is enabled with one line, and it’s environment-aware — just like a production-grade API should be.
 */