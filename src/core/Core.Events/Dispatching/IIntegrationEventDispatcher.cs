using Core.Events.Abstractions;

namespace Core.Events.Dispatching;

/*
 This interface is used in event-driven microservices to dispatch integration events, usually across service boundaries.
 */

/*
 IIntegrationEventDispatcher is an abstraction for dispatching integration events.

Defines a single responsibility: send an integration event to its subscribers or a message bus.

Makes the system decoupled and testable, because your code depends on an interface rather than a concrete implementation.
 */

public interface IIntegrationEventDispatcher
{
    /*
     Parameters

IIntegrationEvent integrationEvent

The event to dispatch.

Represents something that happened in one service and other services might care about.

Could include properties like:

Id → Unique event identifier.

OccurredOn → Timestamp.

Business payload → e.g., OrderId, CustomerId.

CancellationToken cancellationToken = default

Optional token to cancel the asynchronous dispatch operation.

Return Type

Task → Indicates asynchronous execution, which is important when publishing events over a message broker or event bus.
     */
    Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}

/*
 Purpose of IIntegrationEventDispatcher

Decouple services

The service raising the event doesn’t need to know who will consume it.

Consumers subscribe independently, often through a message bus or queue.

Centralize integration event dispatching

Provides a single entry point for sending all integration events.

Handles the mechanics of publishing to a bus or broker (e.g., RabbitMQ, Kafka, Azure Service Bus).

Support asynchronous, reliable delivery

Integration events are usually sent asynchronously, allowing services to scale independently.

Dispatcher can implement retry policies or error handling.

Enable eventual consistency

Supports cross-service workflows without requiring synchronous calls.

Example: OrderPlacedIntegrationEvent → triggers inventory update, billing, and notification services independently.
 */

/*
 Summary

IIntegrationEventDispatcher is an interface for dispatching integration events across services.

Purpose:

Decouple service that raises the event from those that consume it.

Provide a central, consistent way to publish events.

Enable asynchronous and reliable delivery via a message bus.

Support event-driven microservice architectures and eventual consistency.
 */
