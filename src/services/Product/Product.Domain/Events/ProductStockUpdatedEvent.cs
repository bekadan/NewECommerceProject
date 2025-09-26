using Core.Events.Abstractions;

namespace Product.Domain.Events;

public sealed class ProductStockUpdatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public int OldStock { get; }
    public int NewStock { get; }
    public DateTime OccurredOn { get; }

    public ProductStockUpdatedEvent(Guid id, int oldStock, int newStock)
    {
        Id = id;
        OldStock = oldStock;
        NewStock = newStock;
        OccurredOn = DateTime.UtcNow;
    }
}
