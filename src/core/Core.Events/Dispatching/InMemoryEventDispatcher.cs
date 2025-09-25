using Microsoft.Extensions.DependencyInjection;
using Core.Events.Abstractions;
using Core.Events.Handlers;
using Core.Events.Extensions;

namespace Core.Events.Dispatching;

/*
 InMemoryEventDispatcher is a concrete implementation of the IEventDispatcher interface.
 */

/*
 Its main job is to:

Find and execute all event handlers that are registered for a given IDomainEvent.

Do this inside the same process (in-memory), without using external tools like message brokers.

This is a common approach in Domain-Driven Design (DDD) to propagate domain events from one part of the system to another synchronously or asynchronously within the same service.
 */

public class InMemoryEventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public InMemoryEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        /*
         Each domain event has one or more handlers implementing IEventHandler<TEvent>.

Here, we dynamically build the type of handler based on the runtime type of the event.

Example:

If domainEvent is OrderPlacedEvent, then handlerType becomes IEventHandler<OrderPlacedEvent>.
         */
        var handlerType = typeof(IEventHandler<>).MakeGenericType(domainEvent.GetType());

        /*
         GetServices(handlerType) asks the DI container for all services implementing this handler type.

If multiple handlers exist for the same event, all of them will be returned.

.Cast<IEventHandler<IDomainEvent>>() allows us to treat them generically so we can invoke them.
         */
        var handlers = _serviceProvider.GetServices(handlerType)
                                       .Cast<IEventHandler<IDomainEvent>>();

        /*
         Loop through all resolved handlers.

InvokeHandlerAsync is likely an extension method that calls HandleAsync() on the handler.

Each handler runs its logic in response to the event.*/

        foreach (var handler in handlers)
        {
            await handler.InvokeHandlerAsync(domainEvent, cancellationToken);
        }
    }
}

/*
 When DispatchAsync(new OrderPlacedEvent(orderId)) is called:

Dispatcher builds the handler type: IEventHandler<OrderPlacedEvent>

It asks DI container: “Give me all registered IEventHandler<OrderPlacedEvent>.”

Finds SendEmailWhenOrderPlacedHandler

Invokes HandleAsync()

✅ Result: Email gets sent automatically without the event publisher needing to know anything about the handler.
 */

/*
 * Why Use an In-Memory Dispatcher?
 * 
 | Feature            | Purpose                                                                                            |
| ------------------ | -------------------------------------------------------------------------------------------------- |
| **Loose coupling** | The event publisher doesn’t know who handles the event.                                            |
| **Scalability**    | Multiple handlers can react independently.                                                         |
| **Testability**    | Easy to test handlers individually.                                                                |
| **Simplicity**     | No external queue or message bus needed (good for monoliths or single services).                   |
| **Flexibility**    | Can later replace with a distributed dispatcher (e.g., Kafka, RabbitMQ) with minimal code changes. |

 */

/*
 SUMMARY

InMemoryEventDispatcher is the “event delivery system” inside your application.
Its main responsibilities are:

✅ Receive a domain event from anywhere in the code.

✅ Find all handlers registered for that event type using dependency injection.

✅ Execute each handler asynchronously.

It’s a core building block in a clean DDD architecture for enabling decoupled, event-driven communication within a single service.
 */


