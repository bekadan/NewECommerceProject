using Azure;
using Core.Events.Dispatching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Events;

/*
 it's a bootstrapper for your eventing infrastructure. This is where your application decides which event dispatching strategy 

to use (e.g., in-memory for development/tests, or Azure Event Grid for production).



1. Purpose of DependencyInjection

The purpose of this class is to:

📦 Register core event infrastructure services into the .NET DI container.

🔁 Dynamically decide which implementation to use based on configuration:

✅ Azure Event Grid for real cloud event publishing.

✅ In-memory event dispatcher for local development, testing, or simpler setups.

This allows you to swap event dispatching backends without changing code, just by flipping a configuration flag.

 */

public static class DependencyInjection
{
    public static IServiceCollection AddCoreEvents(this IServiceCollection services, IConfiguration configuration)
    {
        // Choose the Eventing Backend Dynamically
        // Reads the config key Events:UseEventGrid (from appsettings.json, environment variables, or secrets).
        var useEventGrid = configuration.GetValue<bool>("Events:UseEventGrid");

        /*
         Determines which backend to register:

true → Use Azure Event Grid (production-grade, cloud-native).

false → Use in-memory dispatcher (for dev/tests).
         */

        if (useEventGrid)
        {
            // Azure Event Grid Configuration

            /*
             Validates required configuration values — throws clear exceptions if they're missing.

Creates an AzureKeyCredential to authenticate with Azure Event Grid.

Registers AzureEventGridDispatcher as a singleton implementing IIntegrationEventDispatcher.

            What this means:

Your system can now publish events to Azure Event Grid from anywhere via IIntegrationEventDispatcher.

The dispatcher will handle retry policies, serialization, etc., internally (as we saw in AzureEventGridDispatcher).
             */
            var endpoint = configuration["Events:EventGridEndpoint"]
                           ?? throw new InvalidOperationException("Events:EventGridEndpoint not set");
            var key = configuration["Events:EventGridKey"]
                      ?? throw new InvalidOperationException("Events:EventGridKey not set");
            var credential = new AzureKeyCredential(key);

            services.AddSingleton<IIntegrationEventDispatcher>(sp =>
                new AzureEventGridDispatcher(endpoint, credential));
        }
        else
        {
            // Fallback: In-Memory Dispatcher

            /*
             If UseEventGrid is false, we register a local in-memory dispatcher.

This is useful for:

✅ Unit tests (no external dependencies)

✅ Local development (no need for cloud resources)

✅ Simple services that only need in-process domain events

Note: This example registers IEventDispatcher instead of IIntegrationEventDispatcher.
This suggests the in-memory version is used for domain events rather than external integration events.
             */
            services.AddSingleton<IEventDispatcher, InMemoryEventDispatcher>();
        }

        return services;
    }

}

/*
 Why This Pattern Is Important
 This pattern follows clean architecture principles and is extremely common in professional .NET microservices:

| Benefit                          | Explanation                                                  |
| -------------------------------- | ------------------------------------------------------------ |
| 🪄 **Configuration-driven**      | Change the event backend without touching code.              |
| 🔄 **Swappable Implementations** | Use in-memory for dev/tests, cloud for production.           |
| ☁️ **Cloud-ready**               | Integrates Azure Event Grid seamlessly.                      |
| 🧪 **Testable**                  | Keeps testing lightweight by avoiding external dependencies. |
| 📦 **Centralized registration**  | Easy to manage and scale as your event infrastructure grows. |

Best Practices

✅ Always validate config values (as done with InvalidOperationException).

✅ Keep production and dev/test configurations separate (e.g., using ASPNETCORE_ENVIRONMENT).

✅ Prefer IIntegrationEventDispatcher for external events and IEventDispatcher for domain events — this keeps the architecture clear.

Summary

The DependencyInjection.AddCoreEvents() method is the gateway to your entire eventing system. It’s small, but it’s one of the most critical pieces of infrastructure setup:

🌐 Dynamically selects between Azure Event Grid (production) and In-memory dispatching (dev/test).

📦 Registers everything into the DI container cleanly and consistently.

🔁 Allows your system to evolve easily — swap providers without code changes.

🧪 Keeps your architecture clean, testable, and maintainable.

 */

/*
 {
  "Events": {
    "UseEventGrid": true,
    "EventGridEndpoint": "https://<your-topic-name>.<region>-1.eventgrid.azure.net/api/events",
    "EventGridKey": "<your-access-key>"
  }
}

// Add Core Events package
builder.Services.AddCoreEvents(builder.Configuration);
 
 */
