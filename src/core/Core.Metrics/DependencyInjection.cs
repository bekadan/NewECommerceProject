using Core.Metrics.Abstractions;
using Core.Metrics.Configurations;
using Core.Metrics.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace Core.Metrics;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind appsettings section
        var aiOptions = new ApplicationInsightsOptions();
        configuration.GetSection("ApplicationInsights").Bind(aiOptions);

        if (string.IsNullOrWhiteSpace(aiOptions.ConnectionString))
            throw new InvalidOperationException("Application Insights ConnectionString is not configured.");

        // Register TelemetryConfiguration using ConnectionString
        services.AddSingleton(sp =>
        {
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            telemetryConfig.ConnectionString = aiOptions.ConnectionString;
            return telemetryConfig;
        });

        // Register TelemetryClient
        services.AddSingleton(sp =>
        {
            var telemetryConfig = sp.GetRequiredService<TelemetryConfiguration>();
            return new Microsoft.ApplicationInsights.TelemetryClient(telemetryConfig);
        });

        // Register IMetricsCollector
        services.AddSingleton<IMetricsCollector, ApplicationInsightsMetricsCollector>();

        return services;
    }
}

/*
 {
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=YOUR-INSTRUMENTATION-KEY-HERE;IngestionEndpoint=https://<region>.in.applicationinsights.azure.com/",
    "EnableAdaptiveSampling": true,
    "EnableQuickPulseMetricStream": true
  }
}

 
 */
