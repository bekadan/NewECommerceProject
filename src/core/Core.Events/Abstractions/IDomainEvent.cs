namespace Core.Events.Abstractions
{
    /*
     Purpose of a Domain Event

Represent something that happened in the domain

A domain event is a meaningful occurrence in the business domain.

Example:

OrderPlacedEvent → An order was successfully placed.

PaymentReceivedEvent → Payment for an invoice was completed.

Decouple event producers from consumers

The entity or service that triggers the event doesn’t need to know who will handle it.

Handlers (IEventHandler<TEvent>) can independently process events.

Enable asynchronous reactions

Events allow different parts of the system to react without blocking the main workflow.

Example: After an OrderPlacedEvent:

Send a confirmation email.

Update analytics.

Adjust inventory.

Improve traceability and auditing

Each event is a record of something that happened in the system.

You can store events for audit logs or event sourcing.
     */
    public interface IDomainEvent
    {
        DateTime OccuredOnUtc { get; }
    }
}

/*
 IDomainEvent marks a class as a domain event in your system.

Purpose:

Represent something important that happened in the domain.

Decouple producers and consumers.

Enable asynchronous reactions and workflows.

Support auditing and event sourcing.
 */
