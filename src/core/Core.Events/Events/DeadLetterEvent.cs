using Core.Events.Abstractions;
using System.Text.Json;

namespace Core.Events.Events;

public class DeadLetterEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public string OriginalEventType { get; }
    public string OriginalEventJson { get; }
    public string ErrorMessage { get; }

    public DeadLetterEvent(IIntegrationEvent originalEvent, string errorMessage)
    {
        OriginalEventType = originalEvent.GetType().FullName!;
        OriginalEventJson = JsonSerializer.Serialize(originalEvent);
        ErrorMessage = errorMessage;
    }
}
