using Core.Events.Abstractions;

namespace Core.Events.Events;

public record DeadLetterIntegrationEvent(
    object originalEvent,
    string eventType,
    DateTime failedAt,
    string exceptionMessage,
    string? stackTrace) : IIntegrationEvent
{
    public Guid Id { get; }

    public DateTime OccurredOn { get; }
}
