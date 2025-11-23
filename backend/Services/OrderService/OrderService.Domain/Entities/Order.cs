namespace OrderService.Domain.Entities;

/// <summary>
/// Order aggregate root.
/// Represents a customer order with line items and status tracking.
/// </summary>
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<OrderLineItem> _lineItems = new();
    public IReadOnlyList<OrderLineItem> LineItems => _lineItems.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order() { } // EF Core

    /// <summary>
    /// Factory method to create a new order.
    /// </summary>
    public static Order Create(Guid customerId, List<OrderLineItem> lineItems)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        if (lineItems == null || lineItems.Count == 0)
            throw new ArgumentException("Order must have at least one line item", nameof(lineItems));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = lineItems.Sum(li => li.Quantity * li.UnitPrice)
        };

        order._lineItems.AddRange(lineItems);
        order._domainEvents.Add(new OrderCreatedEvent(order.Id, customerId, order.TotalAmount, lineItems.Count));

        return order;
    }

    /// <summary>
    /// Confirm the order (payment processed).
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order in {Status} status");

        Status = OrderStatus.Confirmed;
        _domainEvents.Add(new OrderConfirmedEvent(Id));
    }

    /// <summary>
    /// Ship the order.
    /// </summary>
    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot ship order in {Status} status");

        Status = OrderStatus.Shipped;
        CompletedAt = DateTime.UtcNow;
        _domainEvents.Add(new OrderShippedEvent(Id, trackingNumber));
    }

    /// <summary>
    /// Cancel the order.
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Completed)
            throw new InvalidOperationException($"Cannot cancel order in {Status} status");

        Status = OrderStatus.Canceled;
        _domainEvents.Add(new OrderCanceledEvent(Id));
    }

    /// <summary>
    /// Mark order as completed.
    /// </summary>
    public void Complete()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot complete order in {Status} status");

        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        _domainEvents.Add(new OrderCompletedEvent(Id));
    }

    /// <summary>
    /// Clear domain events after publishing.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Order line item value object.
/// </summary>
public class OrderLineItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid OrderId { get; set; }

    private OrderLineItem() { } // EF Core

    public static OrderLineItem Create(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        return new OrderLineItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    public decimal GetTotal() => Quantity * UnitPrice;
}

/// <summary>
/// Order status enumeration.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Completed = 3,
    Canceled = 4,
    Failed = 5
}

/// <summary>
/// Base interface for domain events.
/// </summary>
public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTime OccurredAt { get; }
}

public record OrderCreatedEvent(Guid AggregateId, Guid CustomerId, decimal TotalAmount, int LineItemCount) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record OrderConfirmedEvent(Guid AggregateId) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record OrderShippedEvent(Guid AggregateId, string TrackingNumber) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record OrderCanceledEvent(Guid AggregateId) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}

public record OrderCompletedEvent(Guid AggregateId) : IDomainEvent
{
    public DateTime OccurredAt => DateTime.UtcNow;
}
