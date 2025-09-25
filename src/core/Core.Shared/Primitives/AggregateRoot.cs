
namespace Core.Shared.Primitives;

/*
 1️⃣ What is an Aggregate?

An Aggregate is a cluster of related domain objects (Entities and Value Objects) that are treated as a single unit for data changes.

It enforces consistency boundaries—all changes to the aggregate should leave it in a valid state.

Example:

Order aggregate:

Root: Order (Aggregate Root)

Entities: OrderLine (multiple order lines)

Value Objects: Money, ProductId

The aggregate ensures that, for example, the total order price always matches the sum of order lines.

2️⃣ What is an Aggregate Root?

The Aggregate Root (AR) is the main entity in the aggregate.

It is the only object that outside code can directly interact with.

All other entities in the aggregate are internal and protected—they are only modified through the AR.

Rules:

Only the Aggregate Root can be retrieved directly from a repository.

Only the Aggregate Root can enforce invariants across the aggregate.

All changes inside the aggregate should happen through methods on the root, not directly on internal entities.

Instructor analogy:

Think of the aggregate root as a guardian of a kingdom. Outside code can only go through the guardian to make changes. 
The subjects inside (entities/value objects) cannot be accessed directly.


 */

/*
 3️⃣ Aggregate Root responsibilities

Maintain invariants

Example: An Order cannot have a negative total price.

Control access to internal entities

Example: You cannot modify an OrderLine directly. You must call Order.AddLine(...).

Track domain events (optional but common)

Example: Raising OrderPlacedEvent when a new order is placed.

Encapsulate business logic

The aggregate root is the behavioral entry point, not just a data holder.
 */

/*
 4️⃣ Why Aggregate Roots are important

Consistency: Ensures that all related changes are valid before persisting.

Isolation: Prevents external code from breaking internal rules.

DDD alignment: Maps directly to bounded contexts, reflecting real business rules.
 */

/*
 6️⃣ Key takeaways

Aggregate Root is the “entry point” for the aggregate.

It enforces business rules and consistency.

Internal entities/value objects are protected behind the root.

Aggregate Root is usually the unit of persistence in repositories.

It can raise domain events, but doesn’t have to know about event handling infrastructure.
 */

/* ################################# */

/*
 abstract → This class cannot be instantiated directly. You create domain-specific aggregates by inheriting from it.

<TEvent> → This is a generic type parameter, meaning the aggregate can handle any type of event you choose (e.g., IDomainEvent in your domain).

where TEvent : class → Ensures that TEvent is a reference type (cannot be a struct/value type). This is typical for events because events are usually objects.
 
 Using generics here is key—it lets your Core.Shared remain independent of Core.Events, while still supporting domain events.


 */




public abstract class AggregateRoot<TEvent> where TEvent : class
{
    /*
     2️⃣ Internal list of events
    _events is a private backing field that stores all events that happen inside the aggregate.

readonly → The reference to the list cannot change, ensuring integrity (you can still add/remove items, but cannot replace the list itself).

Why private?
You don’t want external code to arbitrarily modify the event list. All changes should go through the AggregateRoot’s methods.
     */
    private readonly List<TEvent> _events = new();

    /*
     3️⃣ Public read-only exposure

    Exposes the events as read-only to external code.

Consumers (like an event dispatcher) can inspect the events, but cannot modify them directly.

Instructor note: This ensures encapsulation—the aggregate is in full control of its state changes.
     */
    public IReadOnlyCollection<TEvent> Events => _events;

    /*
     4️⃣ Method to add events

    protected → Only the aggregate itself (or derived classes) can add events. External code cannot directly add events to an aggregate.

@event → The @ allows using a C# keyword (event) as a variable name.

_events.Add(@event) → Adds the event to the internal list.

Instructor note: This is the mechanism by which aggregates “raise events”. Any time something happens inside the aggregate that 
    
    should be communicated (like OrderPlaced, ProductStockDecreased, etc.), you call AddEvent.
     */
    protected void AddEvent(TEvent @event) => _events.Add(@event);
}

/*
 5️⃣ How this fits DDD best practices

Core.Shared doesn’t know anything about your actual IDomainEvent types.

The dependency flows correctly: domain-specific aggregates define the concrete event type.

Aggregates can still be unit tested independently—you can verify that the right events were raised without any infrastructure code.
 */

/*
 ✅ Summary as an instructor:

“This AggregateRoot<TEvent> is a generic, infrastructure-agnostic base class. It provides a safe, encapsulated way for aggregates to track domain events without creating a 

direct dependency on a specific event library. By using generics, it achieves reusability and isolation, which are core DDD principles.”
 */
