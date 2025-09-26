using Core.Events.Abstractions;

namespace Core.Integration.Events;

/*
 Purpose: Defines a contract for publishing and subscribing to integration events.

This is infrastructure-agnostic — the interface doesn’t know if you use RabbitMQ, Kafka, Azure Service Bus, etc.
 */

public interface IIntegrationEventBus : IDisposable
{
    /*
     TEvent : IIntegrationEvent

    Benefits:

Type safety

Only objects implementing IIntegrationEvent can be published or subscribed.

Prevents accidental misuse.

Semantic clarity

Makes it clear that this bus is for integration events only, not for arbitrary classes.

Better DDD alignment

Domain layer defines domain events.

Integration layer defines integration events.

Using the interface ensures correct separation of concerns.

    “Generics with interface constraints enforce correct usage at compile time, not runtime.”

    This is especially important in DDD microservices, because integration events are critical for eventual consistency.
     */

    /*
     Summary

    Summary

✅ Constraining TEvent : IIntegrationEvent is best practice.

It enforces correctness, improves readability, and aligns with DDD principles.

Keep the interface in Core.Events, but bus implementation in Core.Integration.
     */

    /*
     What it does: Sends an integration event to all interested subscribers.

Generic TEvent constraint:

Must implement IIntegrationEvent from Core.Events.

Ensures type safety: only valid events can be published.

    Example : 

    var orderPlacedEvent = new OrderPlacedIntegrationEvent(order.Id);
await integrationEventBus.PublishAsync(orderPlacedEvent);


    Step by step flow in DDD context:

Domain triggers a Domain Event (OrderPlacedDomainEvent).

Application layer maps it to an Integration Event (OrderPlacedIntegrationEvent).

PublishAsync sends it through the integration bus.

Other microservices subscribed to this event (e.g., Inventory, Shipping) react asynchronously.
     */

    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IIntegrationEvent;

    /*
     What it does: Registers a handler to react when an integration event is received.

Generic TEvent constraint:

Only accepts valid IIntegrationEvent.

Usage Example:

    integrationEventBus.SubscribeAsync<OrderPlacedIntegrationEvent>(async evt =>
{
    await inventoryService.ReserveStockAsync(evt.OrderId);
});

    Step by step flow:

Microservice subscribes to a specific integration event type.

Whenever an event is published, the bus calls the handler function.

The handler executes application logic (e.g., updating stock, sending notification).

     */

    Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : IIntegrationEvent;
}

/*
 5. Benefits in DDD

Decouples microservices

Services don’t call each other directly; they communicate via events.

Helps maintain bounded contexts.

Supports eventual consistency

Orders can be processed in one service while stock or payment updates happen asynchronously.

Testability

You can mock IIntegrationEventBus in unit tests without starting RabbitMQ/Kafka.

Infrastructure independence

Code only depends on the interface, not RabbitMQ/Kafka/Azure Service Bus specifics.
 */

/*
 
 6. Typical Usage Flow in Ecommerce
 
Order Service publishes an event:
await _integrationEventBus.PublishAsync(new OrderPlacedIntegrationEvent(order.Id));

Inventory Service subscribes:
_integrationEventBus.SubscribeAsync<OrderPlacedIntegrationEvent>(async evt =>
{
    await _inventoryService.ReserveStockAsync(evt.OrderId);
});

Shipping Service subscribes:
_integrationEventBus.SubscribeAsync<OrderPlacedIntegrationEvent>(async evt =>
{
    await _shippingService.SchedulePickupAsync(evt.OrderId);
});

Each service reacts independently, maintaining loose coupling. That's all. We will see the examples later on the course.
 */

/*
 * 
 * Domain Events → inside your microservice, synchronous, part of domain logic
 Integration Events → cross-service communication, asynchronous, infrastructure concern

Use IIntegrationEventBus as the bridge between these worlds.
 */

