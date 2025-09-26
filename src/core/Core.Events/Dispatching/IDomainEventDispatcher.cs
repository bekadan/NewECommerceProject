using Core.Events.Abstractions;

namespace Core.Events.Dispatching;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
