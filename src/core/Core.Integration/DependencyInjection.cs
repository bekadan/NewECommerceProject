using Core.Integration.Abstractions;
using Core.Integration.Events;
using Core.Integration.Http;
using Core.Integration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Integration;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreIntegration(this IServiceCollection services)
    {
        services.AddHttpClient<IHttpClientWrapper, HttpClientWrapper>();
        services.AddTransient<IEmailService, SmtpEmailService>();
        services.AddSingleton<IIntegrationEventBus, RabbitMqIntegrationEventBus>();

        return services;
    }
}
