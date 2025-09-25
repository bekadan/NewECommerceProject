using Core.Events.Abstractions;

namespace Core.Events.Handlers;

/*
 
 Breaking it down:

public interface IEventHandler<in TEvent>

Declares a generic interface for handling events.

TEvent is the type of event it handles.

The in keyword means contravariance, which allows a handler of a base event type to also handle derived event types.

where TEvent : IDomainEvent

Constrains TEvent to implement IDomainEvent.

Ensures this interface only handles domain events, not arbitrary types
 */

public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /*
     Purpose:

Defines a single method that must be implemented by any event handler.

Parameters:

TEvent domainEvent → The event instance to handle.

CancellationToken cancellationToken = default → Allows cancelling the operation if needed.

Returns:

Task → Asynchronous operation, suitable for async handling of events, such as:

Sending an email.

Updating a read model.

Publishing to a message bus.
     */

    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

/*
 SUMMARY

IEventHandler<TEvent> is a generic interface for handling domain events asynchronously.

Guarantees type safety: handlers only handle objects that implement IDomainEvent.

Enables loose coupling: any number of event handlers can process events without the publisher knowing them.

The in keyword allows more flexible type assignments (handlers for base events can handle derived events)
 
 */