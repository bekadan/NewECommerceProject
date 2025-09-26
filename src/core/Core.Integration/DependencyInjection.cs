using Core.Integration.Abstractions;
using Core.Integration.Events;
using Core.Integration.Http;
using Core.Integration.Options;
using Core.Integration.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Integration;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpClient and email service
        services.AddHttpClient<IHttpClientWrapper, HttpClientWrapper>();
        services.AddTransient<IEmailService, SmtpEmailService>();

        // Bind RabbitMQ options
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        // TelemetryClient
        services.AddSingleton<TelemetryClient>();

        // Hosted service for RabbitMQ bus
        services.AddSingleton<RabbitMqIntegrationEventBusHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<RabbitMqIntegrationEventBusHostedService>());

        // Optional: register bus instance for DI consumers
        services.AddSingleton<IIntegrationEventBus>(provider =>
            provider.GetRequiredService<RabbitMqIntegrationEventBusHostedService>().Bus
        );

        return services;
    }
}

/*
 // Add configuration and DI
builder.Services.AddCoreIntegration(builder.Configuration);
 */

/*
 "RabbitMq": {
  "HostName": "localhost",
  "DlqExchangeName": "my-service.dlx",
  "MaxRetryAttempts": 3,
  "BaseRetryDelay": "00:00:05",
  "EnableTelemetry": true
}
 */