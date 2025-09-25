using Core.BackgroundJobs.Abstractions;
using Core.BackgroundJobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Core.BackgroundJobs;

internal class BackgroundJobInitializer : IHostedService
{
    private readonly IServiceProvider _provider;

    public BackgroundJobInitializer(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<BackgroundJobProcessor>();

        var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBackgroundJobHandler<>)));

        foreach (var handlerType in handlerTypes)
        {
            var eventInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBackgroundJobHandler<>));

            var eventType = eventInterface.GetGenericArguments()[0];

            var method = typeof(BackgroundJobProcessor)
                .GetMethod(nameof(BackgroundJobProcessor.SubscribeToEvent))!
                .MakeGenericMethod(eventType);

            method.Invoke(processor, null);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
