using Core.Events.Abstractions;

namespace Core.BackgroundJobs.Abstractions;

public interface IBackgroundJobHandler<TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
