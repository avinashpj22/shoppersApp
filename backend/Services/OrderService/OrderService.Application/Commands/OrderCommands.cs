using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Commands;

/// <summary>
/// Command to create a new order.
/// CQRS Command pattern with MediatR.
/// </summary>
public record PlaceOrderCommand(
    Guid CustomerId,
    List<OrderLineItemDto> LineItems
) : IRequest<OrderDto>;

/// <summary>
/// Handler for PlaceOrderCommand.
/// </summary>
public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for customer: {CustomerId}", request.CustomerId);

        // 1. Convert DTOs to domain entities
        var lineItems = request.LineItems.Select(li =>
            OrderLineItem.Create(li.ProductId, li.ProductName, li.Quantity, li.UnitPrice)
        ).ToList();

        // 2. Create aggregate
        var order = Order.Create(request.CustomerId, lineItems);

        // 3. Persist
        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        // 4. Publish domain events to Service Bus
        // Other services (Product, Payment, etc.) subscribe to these events
        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        order.ClearDomainEvents();

        _logger.LogInformation("Order created: {OrderId}, Total: {TotalAmount}", order.Id, order.TotalAmount);

        return MapToDto(order);
    }

    private OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        CompletedAt = order.CompletedAt,
        LineItems = order.LineItems.Select(li => new OrderLineItemDto
        {
            ProductId = li.ProductId,
            ProductName = li.ProductName,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice
        }).ToList()
    };
}

/// <summary>
/// Command to confirm an order (after payment processing).
/// </summary>
public record ConfirmOrderCommand(Guid OrderId) : IRequest<OrderDto>;

/// <summary>
/// Handler for ConfirmOrderCommand.
/// This handler would be triggered by a PaymentProcessed event from the Payment service.
/// </summary>
public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ConfirmOrderCommandHandler> _logger;

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        ILogger<ConfirmOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} not found");

        order.Confirm();

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        order.ClearDomainEvents();

        _logger.LogInformation("Order confirmed: {OrderId}", order.Id);

        return MapToDto(order);
    }

    private OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        CompletedAt = order.CompletedAt,
        LineItems = order.LineItems.Select(li => new OrderLineItemDto
        {
            ProductId = li.ProductId,
            ProductName = li.ProductName,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice
        }).ToList()
    };
}

/// <summary>
/// Command to ship an order.
/// </summary>
public record ShipOrderCommand(Guid OrderId, string TrackingNumber) : IRequest<OrderDto>;

/// <summary>
/// Handler for ShipOrderCommand.
/// </summary>
public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ShipOrderCommandHandler> _logger;

    public ShipOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        ILogger<ShipOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} not found");

        order.Ship(request.TrackingNumber);

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        order.ClearDomainEvents();

        _logger.LogInformation("Order shipped: {OrderId}, Tracking: {TrackingNumber}", order.Id, request.TrackingNumber);

        return MapToDto(order);
    }

    private OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        CompletedAt = order.CompletedAt,
        LineItems = order.LineItems.Select(li => new OrderLineItemDto
        {
            ProductId = li.ProductId,
            ProductName = li.ProductName,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice
        }).ToList()
    };
}

/// <summary>
/// Command to cancel an order.
/// </summary>
public record CancelOrderCommand(Guid OrderId) : IRequest<OrderDto>;

/// <summary>
/// Handler for CancelOrderCommand.
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} not found");

        order.Cancel();

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        order.ClearDomainEvents();

        _logger.LogInformation("Order cancelled: {OrderId}", order.Id);

        return MapToDto(order);
    }

    private OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        CompletedAt = order.CompletedAt,
        LineItems = order.LineItems.Select(li => new OrderLineItemDto
        {
            ProductId = li.ProductId,
            ProductName = li.ProductName,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice
        }).ToList()
    };
}

// ============================================================================
// Interfaces and DTOs
// ============================================================================
