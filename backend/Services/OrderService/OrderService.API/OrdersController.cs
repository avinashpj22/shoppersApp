using Microsoft.AspNetCore.Mvc;
using MediatR;
using OrderService.Application.Commands;
using OrderService.Application.Queries;

namespace OrderService.API.Controllers;

/// <summary>
/// Order API Controller.
/// Demonstrates REST endpoints for order management.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get an order by ID.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details with line items</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/orders/{OrderId}", id);

        var query = new GetOrderQuery(id);
        var order = await _mediator.Send(query, cancellationToken);

        if (order == null)
            return NotFound(new { message = $"Order {id} not found" });

        return Ok(order);
    }

    /// <summary>
    /// Get all orders for a customer.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of customer orders</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetCustomerOrders(
        Guid customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GET /api/v1/orders/customer/{CustomerId}?page={Page}&pageSize={PageSize}",
            customerId, pageNumber, pageSize);

        var query = new GetCustomerOrdersQuery(customerId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get orders by status (admin dashboard query).
    /// </summary>
    /// <param name="status">Order status (Pending, Confirmed, Shipped, Completed, Canceled)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of orders with given status</returns>
    [HttpGet("status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrdersByStatus(
        string status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GET /api/v1/orders/status/{Status}?page={Page}&pageSize={PageSize}",
            status, pageNumber, pageSize);

        var query = new GetOrdersByStatusQuery(status, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get order statistics (dashboard/analytics).
    /// </summary>
    /// <returns>Order statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderStatisticsDto>> GetStatistics(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/orders/statistics");

        var query = new GetOrderStatisticsQuery();
        var statistics = await _mediator.Send(query, cancellationToken);

        return Ok(statistics);
    }

    /// <summary>
    /// Create a new order.
    /// Initiates the order placement workflow.
    /// </summary>
    /// <param name="request">Order creation request with line items</param>
    /// <returns>Created order with initial status</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "POST /api/v1/orders - Placing order for customer: {CustomerId}",
            request.CustomerId);

        var command = new PlaceOrderCommand(
            request.CustomerId,
            request.LineItems.Select(li => new OrderService.Application.Commands.OrderLineItemDto
            {
                ProductId = li.ProductId,
                ProductName = li.ProductName,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice
            }).ToList()
        );

        try
        {
            var order = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid order data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Confirm an order (after successful payment).
    /// Typically called by a background worker processing PaymentProcessed events.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Confirmed order</returns>
    [HttpPut("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> ConfirmOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/v1/orders/{OrderId}/confirm", id);

        var command = new ConfirmOrderCommand(id);

        try
        {
            var order = await _mediator.Send(command, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Order {id} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Ship an order.
    /// Typically called by fulfillment/logistics system.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Shipping details with tracking number</param>
    /// <returns>Shipped order</returns>
    [HttpPut("{id}/ship")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> ShipOrder(
        Guid id,
        [FromBody] ShipOrderRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "PUT /api/v1/orders/{OrderId}/ship - Tracking: {TrackingNumber}",
            id, request.TrackingNumber);

        var command = new ShipOrderCommand(id, request.TrackingNumber);

        try
        {
            var order = await _mediator.Send(command, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Order {id} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an order.
    /// Only possible if order is not yet shipped.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Canceled order</returns>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CancelOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/v1/orders/{OrderId}/cancel", id);

        var command = new CancelOrderCommand(id);

        try
        {
            var order = await _mediator.Send(command, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Order {id} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Request/Response DTOs
public class PlaceOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<PlaceOrderLineItemRequest> LineItems { get; set; } = new();
}

public class PlaceOrderLineItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class ShipOrderRequest
{
    public string TrackingNumber { get; set; } = string.Empty;
}

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<OrderLineItemDto> LineItems { get; set; } = new();
}

public class OrderLineItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CanceledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}
