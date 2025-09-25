using Azure.Messaging.EventGrid;

namespace Core.Events.Dispatching;

/*
 This interface is used for sending events to Azure Event Grid in an event-driven or cloud-based architecture.
 */

/*
 IEventGridClient is an abstraction over Azure Event Grid operations.

It defines a single responsibility: sending events to Event Grid.

Makes the code testable and decoupled from the actual Azure SDK.
 */
public interface IEventGridClient
{
    /*
     Parameters

EventGridEvent evt

Represents an event that you want to publish to Event Grid.

Contains properties like:

Id → Unique identifier.

EventType → Type of event.

Subject → Context or resource the event relates to.

Data → Payload containing the actual event information.

EventTime → When the event occurred.

CancellationToken cancellationToken = default

Optional token to cancel the asynchronous operation if needed.

Return Type

Task → Indicates asynchronous execution, suitable for cloud operations like sending HTTP requests to Event Grid.
     */
    Task SendEventAsync(EventGridEvent evt, CancellationToken cancellationToken = default);
}

/*
 Purpose of IEventGridClient

Abstract Event Grid communication

Hides the Azure SDK details from your application code.

Allows dependency injection and unit testing.

Send events to Event Grid

Used in microservices to publish domain or integration events to the cloud.

Other services can subscribe to Event Grid topics to react to events.

Support asynchronous, scalable event publishing

Event Grid can handle millions of events per second.

SendEventAsync allows your service to publish events without blocking the main workflow.

Enable loosely coupled architecture

The producer of the event doesn’t need to know the consumers.

Consumers subscribe to Event Grid topics independently.
 */

/*
 Summary

IEventGridClient is an interface for sending events to Azure Event Grid.

Purpose:

Abstract Azure Event Grid SDK for testability and DI.

Publish domain or integration events to a cloud-based event bus.

Enable asynchronous, scalable, and loosely coupled event-driven architectures.

Decouple event producers from consumers subscribing to Event Grid topics.
 */
