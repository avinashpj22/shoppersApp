namespace ProductService.Domain.Entities;

/// <summary>
/// Product aggregate root.
/// Represents a product in the catalog with pricing and inventory.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string Sku { get; private set; }
    public string Category { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Domain events (for event sourcing / event publishing)
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Product() { } // EF Core

    /// <summary>
    /// Factory method to create a new product.
    /// </summary>
    public static Product Create(string name, string description, decimal price, int stockQuantity, string sku, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            Sku = sku,
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        product._domainEvents.Add(new ProductCreatedEvent(product.Id, name, price));
        return product;
    }

    /// <summary>
    /// Update product details.
    /// </summary>
    public void Update(string name, string description, decimal price, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description;
        Price = price;
        Category = category;
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new ProductUpdatedEvent(Id, name, price));
    }

    /// <summary>
    /// Reserve inventory for an order.
    /// </summary>
    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (StockQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");

        StockQuantity -= quantity;
        _domainEvents.Add(new InventoryReservedEvent(Id, quantity, StockQuantity));
    }

    /// <summary>
    /// Release reserved inventory.
    /// </summary>
    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        StockQuantity += quantity;
        _domainEvents.Add(new InventoryReleasedEvent(Id, quantity, StockQuantity));
    }

    /// <summary>
    /// Deactivate product.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new ProductDeactivatedEvent(Id));
    }

    /// <summary>
    /// Clear domain events after publishing.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Base interface for domain events.
/// </summary>
public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTime OccurredAt { get; }
}

public record ProductCreatedEvent(Guid AggregateId, string Name, decimal Price) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record ProductUpdatedEvent(Guid AggregateId, string Name, decimal Price) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record InventoryReservedEvent(Guid AggregateId, int Quantity, int RemainingStock) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record InventoryReleasedEvent(Guid AggregateId, int Quantity, int RemainingStock) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record ProductDeactivatedEvent(Guid AggregateId) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}
