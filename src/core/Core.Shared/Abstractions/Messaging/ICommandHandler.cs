using MediatR;

namespace Core.Shared.Abstractions.Messaging;

/*
 Defines a generic interface for handling commands.

TCommand is the type of command the handler will process.

TResponse is the type of response the handler returns.

The in keyword makes TCommand contravariant, meaning a handler for a base command type can also handle derived command types.
 */

/*
 : IRequestHandler<TCommand, TResponse>

This interface inherits from MediatR’s IRequestHandler<TCommand, TResponse>.

IRequestHandler is the standard MediatR interface for handling requests (commands or queries).

Essentially, ICommandHandler is just a specialized marker interface for commands, instead of generic requests.
 */

/*
 where TCommand : ICommand<TResponse>

Adds a constraint on TCommand:

Only types implementing ICommand<TResponse> can be used as TCommand.

Ensures type safety: you cannot accidentally register a query or a random class as a command handler.
 */

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
where TCommand : ICommand<TResponse>
{
}

/*
 How it works in practice

public class CreateUserCommand : ICommand<Guid>
{
    public string Name { get; set; }
}

Example handler:

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Logic to create a user
        return Guid.NewGuid(); // Example response
    }
}


 */

/*
 SUMMARY

Summary

ICommandHandler<TCommand, TResponse> is a specialized MediatR handler for commands.

TCommand is contravariant (in) — allows flexibility in assigning handlers.

Ensures type safety by constraining TCommand to ICommand<TResponse>.

Keeps your architecture clean and consistent with CQRS principles.
 */