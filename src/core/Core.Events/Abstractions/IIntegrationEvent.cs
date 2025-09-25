namespace Core.Events.Abstractions;

/*
 IIntegrationEvent is typically an interface that marks a class as an integration-level event, meant for communication between services.

Purpose of an Integration Event

Represent something that happened in one service that other services may need to know

Unlike a domain event (which is internal to a bounded context), an integration event is published externally.

Example:

OrderPlacedIntegrationEvent → Published after an order is placed so other microservices (billing, shipping) can react.

Enable communication between microservices

Integration events are often sent through a message broker (e.g., RabbitMQ, Kafka, Azure Service Bus).

Promotes loose coupling between services: services don’t call each other directly.

Support eventual consistency

Systems don’t need to update other services synchronously.

Other services eventually receive the event and update their state.

Track cross-service events

Integration events provide a record of interactions between services for auditing or replay.

 */

/*
 This event can be published to a message bus.

Other microservices can have subscribers/handlers that consume it asynchronously.
 */

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}

/*
 IIntegrationEvent is used for communication between services in a distributed system.

Purpose:

Represent events that other services care about.

Enable loose coupling and async communication.

Support eventual consistency and cross-service workflows.

Can be published and subscribed to over a message bus.
 */
