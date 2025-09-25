using Core.Logging.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Core.Logging;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreLogging(this IServiceCollection services, IConfiguration configuration)
    {
        /*
         Registers your custom ILogger interface and its implementation (Logger) as a singleton.

Singleton ensures one shared instance for the whole application.
         */
        services.AddSingleton<Abstractions.ILogger, Logger>();

        //IHttpContextAccessor allows your logger or enrichers to access HTTP context (e.g., headers, correlation IDs).
        //Registered as a singleton because it’s thread-safe and shared across requests.
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        /*

         Let’s break it down line by line:

new LoggerConfiguration() → Starts a new Serilog logger configuration.

.ReadFrom.Configuration(configuration) → Reads Serilog settings from appsettings.json (like log level, sinks, etc.).

.Enrich.FromLogContext() → Adds contextual information to logs (e.g., request ID, user info).

.Enrich.WithCorrelationIdHeader() → Adds the correlation ID from HTTP headers to every log (requires a custom enricher, like CorrelationIdEnricher we discussed before).

.WriteTo.Console() → Logs messages to the console.

.WriteTo.ApplicationInsights(...) → Logs messages to Azure Application Insights for monitoring.

.CreateLogger() → Builds the Serilog logger and sets it as the global logger.*/

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationIdHeader()
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(configuration["ApplicationInsights:InstrumentationKey"], TelemetryConverter.Traces)
            .CreateLogger();

        return services;
    }
}

/*
 
 Summary

Registers custom logger and HTTP context accessor.

Configures Serilog with:

Appsettings-based configuration.

Contextual enrichers (log context, correlation ID).

Sinks: console and Application Insights.

Provides a clean extension method for DI registration.
 */

/*
 // Register Core.Logging
builder.Services.AddCoreLogging(builder.Configuration);

// Use request/response logging middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();


Purpose: Configure logging levels for Serilog.

Levels hierarchy:
Verbose < Debug < Information < Warning < Error < Fatal

Information logs include regular operational messages.

Ignores Debug or Verbose messages unless explicitly overridden.

Allows specific libraries/namespaces to have different logging levels.

Example:

Microsoft namespace logs Warning or higher.

System namespace logs Warning or higher.

This is useful to reduce noisy logs from framework libraries while keeping your app’s logs at Information.

{
  "ApplicationInsights": {
    "InstrumentationKey": "<your-key>"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}

Purpose: Configure Azure Application Insights, a monitoring and telemetry service.

InstrumentationKey:

Unique key that links your application to a specific Application Insights resource in Azure.

Replace <your-key> with the actual key from Azure.

This key is later read by Serilog to send logs to Application Insights:


SUMMARY

ApplicationInsights → Provides your Azure telemetry key.

Serilog MinimumLevel → Controls which log messages are recorded.

Default = Information → logs info, warnings, errors, and fatal.

Override → reduces verbosity for Microsoft and System namespaces.

Helps filter logs and send them to both console and Application Insights via Serilog

 */