using Core.Events.Abstractions;

namespace Core.Integration.Events;



public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }

    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
