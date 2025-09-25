using Azure;
using Azure.Messaging.EventGrid;
using Core.Events.Abstractions;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace Core.Events.Dispatching;

/*
 this class is key for publishing integration events to Azure Event Grid, a cloud-based event routing service.
 */

/*
 Purpose of AzureEventGridDispatcher

AzureEventGridDispatcher is a real-world implementation of IIntegrationEventDispatcher.
It’s responsible for:

Converting integration events from your application into Event Grid events.

Publishing them to Azure Event Grid so other services, applications, or systems can react asynchronously.

Handling retries and transient failures automatically using Polly.

In short:
✅ Your app → AzureEventGridDispatcher → Azure Event Grid → subscribers (like other microservices, Azure Functions, etc.)
 */

public class AzureEventGridDispatcher : IIntegrationEventDispatcher
{
    /*
     IIntegrationEventDispatcher: The interface requiring a DispatchAsync method.

_client: A wrapper around the Azure Event Grid client that actually sends events.

_retryPolicy: A Polly retry policy to make the dispatch more resilient to transient network or service issues.
     */
    private readonly IEventGridClient _client;
    private readonly AsyncRetryPolicy _retryPolicy;

    // Production constructor
    public AzureEventGridDispatcher(string endpoint, AzureKeyCredential credential)
    {
        /*
         endpoint: The Event Grid topic endpoint URL where events are sent.

credential: The access key to authenticate with Azure Event Grid.

It creates a real EventGridPublisherClient and wraps it in a custom IEventGridClient adapter (EventGridClientWrapper).

It sets up a retry policy:

Retries up to 3 times on any exception.

Waits 2^n seconds between retries (exponential backoff).
         */

        var client = new EventGridPublisherClient(new Uri(endpoint), credential);
        _client = new EventGridClientWrapper(client);

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        
    }

    // Testable constructor
    public AzureEventGridDispatcher(IEventGridClient client)
    {
        _client = client;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        /*
         Allows you to inject a mock client for unit testing.

Follows dependency inversion — makes the class more testable and maintainable.
         */
    }

    public async Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        /*
         subject: A label describing the event. Usually the event class name.

eventType: Type name again (can also be a domain name or schema version).

dataVersion: Version of the event schema.

data: JSON-serialized payload of your IIntegrationEvent.

✅ This transforms your internal event into a standard Event Grid format.
         */
        var eventGridEvent = new EventGridEvent(
            subject: integrationEvent.GetType().Name,
            eventType: integrationEvent.GetType().Name,
            dataVersion: "1.0",
            data: JsonSerializer.Serialize(integrationEvent)
        );

        /*
         Calls the SendEventAsync method on the _client (which wraps EventGridPublisherClient).

Uses Polly retry policy to handle transient network failures or Azure outages.

Retries 3 times with exponential backoff (2s, 4s, 8s).
         */

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _client.SendEventAsync(eventGridEvent, cancellationToken);
        });
    }
}

/*
 * Why Use AzureEventGridDispatcher?
 | Benefit                         | Explanation                                                                    |
| ------------------------------- | ------------------------------------------------------------------------------ |
| **Cross-service communication** | Publishes events to other microservices or external systems.                   |
| **Cloud-native scalability**    | Event Grid handles massive scale and fan-out automatically.                    |
| **Reliable delivery**           | Retry policy and Event Grid’s delivery guarantees ensure the event isn’t lost. |
| **Decoupling**                  | The publisher doesn’t care who listens — services evolve independently.        |
| **Serverless-friendly**         | Azure Functions, Logic Apps, or Event Hubs can react to the event immediately. |

 */

/*
 Summary

AzureEventGridDispatcher is a production-ready implementation of an integration event dispatcher that:

📤 Converts your IIntegrationEvent into an EventGridEvent.

☁️ Publishes it to Azure Event Grid.

🔄 Retries automatically on transient failures.

🧪 Supports dependency injection and unit testing.

It’s a key component for building cloud-native, event-driven microservices that communicate reliably and asynchronously across boundaries.
 */
