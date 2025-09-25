using Core.Events.Abstractions;
using Core.Events.Handlers;

namespace Core.Events.Extensions;
/*
 this is a small but crucial utility in the event dispatching system that makes dynamic event handling possible
 */

/*
 Purpose of EventHandlerExtensions

The main purpose of this class is to allow event dispatchers to call the correct HandleAsync() method on event handlers — even when the event type is only known at runtime.

This solves a common problem in event-driven systems:

❓ How do you call HandleAsync(TEvent) when you don’t know TEvent at compile time?

This extension method uses C# dynamic to bypass compile-time type checking and invoke the correct generic method dynamically.
 */

public static class EventHandlerExtensions
{
    /*
     this IEventHandler<IDomainEvent> handler:

The this keyword makes it an extension method.

It can be called like handler.InvokeHandlerAsync(...) even though it’s defined outside the interface.

IDomainEvent domainEvent:

The domain event to be handled. Its exact type might only be known at runtime.

CancellationToken:

Standard cancellation support for async operations.
     */
    public static Task InvokeHandlerAsync(this IEventHandler<IDomainEvent> handler, IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        /*
         * 
         * Cast to dynamic
         Dynamic typing in C# defers type resolution to runtime.

This is the key trick:

At compile time, the compiler doesn’t know the exact event type.

At runtime, dynHandler and dynEvent will have their real types.

Example:

If domainEvent is OrderPlacedEvent, then dynEvent will become OrderPlacedEvent at runtime.

If handler is IEventHandler<OrderPlacedEvent>, then dynHandler will become that specific type.

        Invoke the Correct HandleAsync

        Because both variables are dynamic, the runtime matches the correct HandleAsync(TEvent) method.

         */
        dynamic dynHandler = handler;
        dynamic dynEvent = domainEvent;
        return dynHandler.HandleAsync(dynEvent, cancellationToken);
    }
}

/*
 Why This Design Is Powerful

| Feature                    | Explanation                                                      |
| -------------------------- | ---------------------------------------------------------------- |
| ✅ **Runtime flexibility**  | No need to know the event type at compile time.                  |
| ✅ **Loose coupling**       | Dispatcher doesn’t need `switch` statements or type checks.      |
| ✅ **Polymorphic handling** | Supports multiple event types and handlers seamlessly.           |
| ✅ **Cleaner code**         | Removes reflection boilerplate — `dynamic` handles the dispatch. |


Important Notes

Using dynamic has a small performance cost because type resolution happens at runtime — but in event handling, this cost is negligible compared to the benefit of flexibility.

If you have hundreds of thousands of events per second, you might consider a more static dispatching mechanism. But for most DDD systems, this approach is ideal.

SUMMARY

Summary

EventHandlerExtensions.InvokeHandlerAsync is a clever helper method that enables your event dispatching system to:

🔄 Dynamically invoke the correct HandleAsync(TEvent) method

📦 Work with IDomainEvent instances whose type is unknown until runtime

🧱 Keep your dispatcher logic simple and clean

🧪 Stay testable and decoupled without using complex reflection code
 */
