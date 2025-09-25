using Microsoft.Extensions.DependencyInjection;

namespace Core.Validation;

/*
 Provides IServiceCollection and DI extension methods like AddSingleton, AddTransient, etc.

By itself, this does not provide .Scan() — that comes from Scrutor.

You need to also add:

using Scrutor; 

Scrutor is a small library that extends the default DI container to allow assembly scanning and automatic registration of types.
 */

public static class DependencyInjection
{
    public static IServiceCollection AddCoreValidation(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {

        if (assemblies == null || assemblies.Length == 0)
        {
            // Default to Core.Validation assembly
            assemblies = new[] { typeof(DependencyInjection).Assembly };
        }


        /*
         Identify the Assembly
        Uses reflection to get the assembly where this DI class lives (Core.Validation).

This assembly is scanned to find all validator classes automatically.

Avoids registering each validator manually.
         */
        // Scan the assembly containing validators and register them
        var assembly = typeof(DependencyInjection).Assembly;

        /*
         Scan Assembly and Register Validators

        Step by step:

a. services.Scan(...)

Provided by Scrutor.

Allows automatic scanning of assemblies and bulk registration of types.

b. .FromAssemblies(assembly)

Specifies which assembly to scan.

In this case, Core.Validation assembly where your validators live.

c. .AddClasses(classes => classes.AssignableTo(typeof(Abstractions.IValidator<>)))

Finds all concrete classes in the assembly that implement IValidator<T>.

Example: ProductValidator : BaseValidator<Product>, IValidator<Product>.

Skips abstract classes automatically.

d. .AsImplementedInterfaces()

Registers the class as all interfaces it implements.

Example:

        ProductValidator : IValidator<Product>

        After registration: IValidator<Product> resolves to ProductValidator in DI.

        e. .WithTransientLifetime()

Sets the lifetime of the service to Transient.

Validators are stateless, so transient is perfect.

Alternatives:

Scoped → one per request

Singleton → one for the entire application lifetime
         */

        // Registers all classes that implement IValidator<T> as transient
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(Abstractions.IValidator<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());


        //Enables chaining multiple DI registrations in one call.
        return services;
    }
}



/*
 * 
 * Usage Example in Program.cs
 * 
 * 
 * // Add logging
builder.Services.AddCoreLogging();
 // Add validators from multiple assemblies
builder.Services.AddCoreValidation(
    typeof(Core.Validation.DependencyInjection).Assembly,  // Core.Validation
    typeof(MyApp.Domain.DependencyInjection).Assembly,    // Domain Layer
    typeof(MyApp.Application.DependencyInjection).Assembly // Application Layer
);

StepByStep

| Line / Concept                                      | Explanation                                                      |
| --------------------------------------------------- | ---------------------------------------------------------------- |
| `params Assembly[] assemblies`                      | Accepts multiple assemblies to scan for validators               |
| `FromAssemblies(assemblies)`                        | Scans **all provided assemblies**                                |
| `AddClasses(...AssignableTo(typeof(IValidator<>)))` | Finds all classes implementing `IValidator<T>`                   |
| `AsImplementedInterfaces()`                         | Registers as `IValidator<T>` automatically                       |
| `WithTransientLifetime()`                           | Validators are **stateless**, so transient is ideal              |
| Default fallback                                    | If no assemblies provided, it scans **Core.Validation assembly** |


✅ Benefits

Automatic registration: No manual services.AddTransient<ProductValidator>().

Multi-assembly scanning: Works across Domain, Application, and Validation layers.

DDDBest Practices: Clean separation of concerns.

Reusable pipeline: Works in microservices and layered architecture.

Transient lifetime: Perfect for stateless validators.

 */
