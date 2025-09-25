using Azure.Messaging.EventGrid;

namespace Core.Events.Dispatching;

/*
 this is a small but important piece in building a clean, testable event-driven system 👇
 */

/*
 Purpose of EventGridClientWrapper

EventGridClientWrapper is a wrapper (adapter) around the official Azure SDK’s EventGridPublisherClient.
Its purpose is:

✅ To implement your custom interface IEventGridClient, which makes the rest of your code independent of Azure SDK details.

✅ To make your code testable — you can easily mock IEventGridClient in unit tests.

✅ To provide a clean abstraction layer between your application logic and the external Azure Event Grid SDK.

This follows the Dependency Inversion Principle (DIP) — one of the SOLID principles — which says “depend on abstractions, not on concrete implementations.”
 */

public class EventGridClientWrapper : IEventGridClient
{
    private readonly EventGridPublisherClient _client;

    public EventGridClientWrapper(EventGridPublisherClient client)
    {
        _client = client;
    }

    /*
     Let's explain what happens here step-by-step:

✅ The method implements the interface contract (IEventGridClient.SendEventAsync).

📦 It receives an EventGridEvent (a standard Azure SDK object containing subject, event type, data, etc.).

☁️ It simply forwards the call to the real Azure SDK client (_client).

⏱ It returns a Task, so the call is asynchronous — perfect for high-scale event publishing.

Even though it looks simple, this indirection is extremely valuable because your application never talks to EventGridPublisherClient directly — only through IEventGridClient.
     */

    public Task SendEventAsync(EventGridEvent evt, CancellationToken cancellationToken = default)
    {
        return _client.SendEventAsync(evt, cancellationToken);
    }
}

/*
 * Why Use a Wrapper? (Key Benefits)
 | Benefit                          | Explanation                                                                         |
| -------------------------------- | ----------------------------------------------------------------------------------- |
| **Testability**                  | You can mock `IEventGridClient` in unit tests instead of hitting Azure.             |
| **Abstraction**                  | Your domain logic isn’t tightly coupled to Azure SDK classes.                       |
| **Flexibility**                  | If you switch from Event Grid to another event system, you only change the wrapper. |
| **Dependency Inversion (SOLID)** | High-level modules (dispatchers) depend on abstractions, not concrete SDK classes.  |

 
 */

/*
 Summary

EventGridClientWrapper is a lightweight but powerful adapter class.
It wraps Azure’s EventGridPublisherClient behind the IEventGridClient interface so your application benefits from:

🧪 Testability: Easy to mock in unit tests.

🧱 Abstraction: Hides Azure SDK details from your domain code.

🔄 Flexibility: Swappable with other implementations in the future.

📏 Clean Architecture: Follows Dependency Inversion & Interface Segregation principles.
 */
