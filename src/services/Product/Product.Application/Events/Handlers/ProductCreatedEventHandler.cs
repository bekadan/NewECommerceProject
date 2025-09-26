using Core.Events.Abstractions;
using Core.Integration.Events;
using Product.Domain.Events;

namespace Product.Application.Events.Handlers;

public class ProductCreatedDomainHandler : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly IIntegrationEventBus _eventBus;

    public ProductCreatedDomainHandler(IIntegrationEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task HandleAsync(ProductCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        // Convert domain event to integration event
        var integrationEvent = new ProductCreatedIntegrationEvent(
            @event.Id,
            @event.Name,
            @event.Price.Amount,
            @event.Stock,
            @event.OccurredOn
        );

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
