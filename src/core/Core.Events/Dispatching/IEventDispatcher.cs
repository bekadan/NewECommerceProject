using Core.Events.Abstractions;

namespace Core.Events.Dispatching;

/*
 IEventDispatcher is typically an interface that defines a mechanism to send or publish events.

Purpose of IEventDispatcher

Decouple event producers from event handlers

The code that creates the event doesn’t know who will handle it.

Example: OrderService raises an OrderPlacedEvent → dispatcher ensures all subscribed handlers are called.

Centralize event handling

Acts as a single point to send events to their handlers.

Reduces the need for services or entities to know all the handlers explicitly.

Support asynchronous processing

The dispatcher can handle events asynchronously, making the system more scalable.

Multiple handlers can be invoked concurrently if needed.

Enable multiple event types

A generic DispatchAsync<TEvent> method allows dispatching any type of event (IDomainEvent or IIntegrationEvent).
 */

public interface IEventDispatcher
{
    /*
     Parameters:

TEvent domainEvent → The event instance to dispatch.

CancellationToken cancellationToken → Optional, allows canceling the dispatch operation.

Behavior:

Finds all handlers for TEvent.

Calls each handler’s HandleAsync method.

Handles errors or logging as needed.
     */
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

/*
 IEventDispatcher is the central mechanism to send events to handlers.

Purpose:

Decouple event producers from handlers.

Support multiple event handlers for one event.

Enable asynchronous and scalable event processing.

Provides a consistent way to dispatch domain or integration events.
 */