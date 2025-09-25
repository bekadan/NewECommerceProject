using MediatR;

namespace Core.Shared.Abstractions.Messaging;

/*
 namespace Core.Shared.Abstractions.Messaging;

This is just defining a namespace for organizing your code.

Usually, Core.Shared.Abstractions indicates that this is a shared, reusable abstraction, so it can be used across multiple services or layers.
 */

/*
 a) interface ICommand<out TResponse>

This defines a generic interface ICommand that has a type parameter TResponse.

TResponse is covariant (denoted by out), meaning you can use ICommand<BaseType> where ICommand<DerivedType> is expected.

Essentially, it represents a command that will return a response of type TResponse.

b) : IRequest<TResponse>

ICommand<TResponse> inherits from IRequest<TResponse>, which is part of MediatR.

IRequest<TResponse> represents a request that is handled by a handler and returns a response of type TResponse.

c) Why this matters

By inheriting from IRequest<TResponse>, the interface can be sent through MediatR’s IMediator:
 */

/*
 public class CreateUserCommand : ICommand<Guid>
{
    public string Name { get; set; }
}

var userId = await mediator.Send(new CreateUserCommand { Name = "Alice" });

Here, Guid is the TResponse (the command returns the newly created user ID).

 */

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/*
 
Summary

ICommand<TResponse> is a marker interface for all commands in our system.

It inherits MediatR’s IRequest<TResponse>, so commands can be handled by MediatR handlers.

The out keyword allows covariance, making the code more flexible.

Promotes CQRS (Command-Query Responsibility Segregation) style architecture: commands for actions that change state.
 */