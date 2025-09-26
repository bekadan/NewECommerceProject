using Core.Events.Abstractions;
using Product.Domain.ValueObjects;

namespace Product.Domain.Events;

public sealed class ProductPriceUpdatedEvent : IDomainEvent
{
    public ProductPriceUpdatedEvent(Guid id, Price oldPrice, Price newPrice)
    {
        Id = id;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public Price OldPrice { get; set; }
    public Price NewPrice { get; set; }
    public DateTime OccurredOn { get; }
}
