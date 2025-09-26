using Core.Events.Abstractions;
using Product.Domain.ValueObjects;

namespace Product.Domain.Events;

public sealed class ProductUpdatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public string Name { get; }
    public Price Price { get; }
    public int Stock { get; }
    public Guid CategoryId { get; }
    public DateTime OccurredOn { get; }

    public ProductUpdatedEvent(Guid id, string name, Price price, int stock, Guid categoryId)
    {
        Id = id;
        Name = name;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        OccurredOn = DateTime.UtcNow;
    }
}
