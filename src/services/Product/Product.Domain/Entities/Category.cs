using Core.Events.Abstractions;
using Core.Shared.Abstractions;
using Core.Shared.Primitives;

namespace Product.Domain.Entities;

public class Category : AggregateRoot<IDomainEvent>, IAuditableEntity, ISoftDeletableEntity
{
    // Backing fields
    private DateTime _createdOnUtc;
    private DateTime? _modifiedOnUtc;
    private bool _deleted;
    private DateTime? _deletedOnUtc;

    // Entity properties
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    // IAuditableEntity / ISoftDeletableEntity
    public DateTime CreatedOnUtc => _createdOnUtc;
    public DateTime? ModifiedOnUtc => _modifiedOnUtc;
    public bool Deleted => _deleted;
    public DateTime? DeletedOnUtc => _deletedOnUtc;

    // Navigation property for Product list
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // EF / ORM constructor
    private Category() { }

    // Public constructor
    public Category(string name)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        SetCreated();
    }

    #region Domain Methods

    public void AddProduct(Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        _products.Add(product);
        SetModified();
    }

    public void RemoveProduct(Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        _products.Remove(product);
        SetModified();
    }

    public void UpdateName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        SetModified();
    }

    public void MarkDeleted()
    {
        _deleted = true;
        _deletedOnUtc = DateTime.UtcNow;
        SetModified();
    }

    public void SetCreated() => _createdOnUtc = DateTime.UtcNow;
    public void SetModified() => _modifiedOnUtc = DateTime.UtcNow;

    #endregion
}
