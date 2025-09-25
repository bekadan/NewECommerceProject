using Core.Events.Abstractions;

namespace Core.Events.Events;

public class SendEmailEvent : IIntegrationEvent
{
    public string To { get; }
    public string Subject { get; }
    public string Body { get; }
    public Guid Id {get; }
    public DateTime OccurredOn { get; }

    public SendEmailEvent(string to, string subject, string body)
    {
        To = to;
        Subject = subject;
        Body = body;
    }
}
