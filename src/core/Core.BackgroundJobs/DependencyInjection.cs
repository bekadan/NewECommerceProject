using Core.BackgroundJobs.Abstractions;
using Core.BackgroundJobs.Configurations;
using Core.BackgroundJobs.Services;
using Core.BackgroundJobs.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;

namespace Core.BackgroundJobs;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // Register BackgroundJobProcessor
        services.AddSingleton<BackgroundJobProcessor>();

        // Register all handlers dynamically
        services.Scan(scan => scan
            .FromAssemblyOf<BackgroundJobProcessor>() // marker type
            .AddClasses(classes => classes.AssignableTo(typeof(IBackgroundJobHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        // Optional: register default retry policy
        services.AddSingleton<AsyncRetryPolicy>(Policies.DefaultRetryPolicy());

        services.Configure<BackgroundJobOptions>(
            configuration.GetSection("BackgroundJobs")
            );

        services.AddHostedService<BackgroundJobInitializer>();

        return services;
    }
}

/*
 "BackgroundJobs": {
  "RetryCount": 3,
  "RetryBaseDelaySeconds": 2,
  "DlqExchange": "my-service.dlx"
}
}
 */