using Core.Events.Abstractions;
using Core.Shared.Abstractions;
using Core.Shared.Primitives;
using Core.Shared.Utility;
using Product.Domain.Events;
using Product.Domain.ValueObjects;

namespace Product.Domain.Entities;

public class Product : AggregateRoot<IDomainEvent>, IAuditableEntity, ISoftDeletableEntity
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
        Ensure.NotLessThan(quantity, 0, "Stock cannot be negative", nameof(quantity));

        var oldStock = Stock;

        Stock = quantity;
        ((IAuditableEntity)this).SetModified(); // explicit call

        AddEvent(new ProductStockUpdatedEvent(Id, oldStock, quantity));
    }

    public void UpdatePrice(Price price)
    {
        var oldPrice = Price;

        
        Ensure.NotEmpty(price.Currency, "Price currency cannot be empty." ,nameof(price.Currency));
        Ensure.NotLessThan(price.Amount, 0, "Amount cannot be less than zero" ,nameof(price.Amount));

        Price = price;

        ((IAuditableEntity)this).SetModified(); // explicit call

        AddEvent(new ProductPriceUpdatedEvent(Id, oldPrice, price));
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
