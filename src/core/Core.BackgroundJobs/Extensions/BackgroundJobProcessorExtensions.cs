using Core.BackgroundJobs.Abstractions;
using Core.BackgroundJobs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.BackgroundJobs.Extensions;

public static class BackgroundJobProcessorExtensions
{
    public static void AutoSubscribeAllEvents(this BackgroundJobProcessor processor, IServiceProvider provider)
    {
        var handlerTypes = provider.GetServices(typeof(IBackgroundJobHandler<>))
            .Select(s => s.GetType());

        foreach (var handlerType in handlerTypes)
        {
            var eventInterface = handlerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBackgroundJobHandler<>));

            if (eventInterface != null)
            {
                var eventType = eventInterface.GetGenericArguments()[0];
                var method = typeof(BackgroundJobProcessor)
                    .GetMethod(nameof(BackgroundJobProcessor.SubscribeToEvent))
                    ?.MakeGenericMethod(eventType);

                method?.Invoke(processor, null);
            }
        }
    }
}