using Core.BackgroundJobs.Abstractions;
using Core.BackgroundJobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Core.BackgroundJobs;

internal class BackgroundJobInitializer : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger _logger;

    public BackgroundJobInitializer(IServiceProvider provider, ILogger logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<BackgroundJobProcessor>();

        var targetAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null &&
                        (a.FullName.Contains("Application") || a.FullName.Contains("Core")))
            .ToList();

        _logger.LogInformation("🔄 Scanning {AssemblyCount} assemblies for background job handlers...", targetAssemblies.Count);

        var handlerTypes = targetAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBackgroundJobHandler<>)))
            .ToList();

        if (!handlerTypes.Any())
        {
            _logger.LogWarning("⚠️ No background job handlers found. Make sure they are registered and public.");
            return Task.CompletedTask;
        }

        foreach (var handlerType in handlerTypes)
        {
            var eventInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBackgroundJobHandler<>));

            var eventType = eventInterface.GetGenericArguments()[0];

            try
            {
                var method = typeof(BackgroundJobProcessor)
                    .GetMethod(nameof(BackgroundJobProcessor.SubscribeToEvent), BindingFlags.Public | BindingFlags.Instance)!
                    .MakeGenericMethod(eventType);

                method.Invoke(processor, null);
                _logger.LogInformation("✅ Subscribed background job for event: {EventName} -> {Handler}", eventType.Name, handlerType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to subscribe handler {Handler} for event {EventName}", handlerType.Name, eventType.Name);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
