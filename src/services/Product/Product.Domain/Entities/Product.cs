using Core.Shared.Abstractions;
using Core.Shared.Primitives;
using Product.Domain.Events;
using Product.Domain.ValueObjects;

namespace Product.Domain.Entities;

public class Product : AggregateRoot<ProductCreatedEvent>, IAuditableEntity, ISoftDeletableEntity
{
    // Backing fields for auditable / soft-delete
    private DateTime _createdOnUtc;
    private DateTime? _modifiedOnUtc;
    private bool _deleted;
    private DateTime? _deletedOnUtc;

    // Entity properties
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Price Price { get; private set; }
    public int Stock { get; private set; }
    public Guid CategoryId { get; private set; }


    public DateTime CreatedOnUtc => _createdOnUtc;
    public DateTime? ModifiedOnUtc => _modifiedOnUtc;

    public bool Deleted => _deleted;
    public DateTime? DeletedOnUtc => _deletedOnUtc;

    // EF / ORM constructor
    private Product() { }

    // Public constructor
    public Product(string name, Price price, int stock, Guid categoryId)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Stock = stock;
        CategoryId = categoryId;

        ((IAuditableEntity)this).SetCreated(); // explicit call

        // Raise creation event
        AddEvent(new ProductCreatedEvent(Id, Name, Price, Stock, CategoryId));
    }

    #region Domain Methods

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(quantity));

        Stock = quantity;
        ((IAuditableEntity)this).SetModified(); // explicit call

        // Raise domain event if needed
        // AddEvent(new ProductStockUpdatedEvent(Id, Stock));
    }

    public void UpdatePrice(Price price)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
        ((IAuditableEntity)this).SetModified(); // explicit call

        // Raise domain event if needed
        // AddEvent(new ProductPriceUpdatedEvent(Id, Price));
    }

    public void MarkDeleted()
    {
        _deleted = true;
        _deletedOnUtc = DateTime.UtcNow;
        ((IAuditableEntity)this).SetModified();
    }

    public void SetCreated()
         => _createdOnUtc = DateTime.UtcNow;

    public void SetModified()
        => _modifiedOnUtc = DateTime.UtcNow;

    #endregion
}
