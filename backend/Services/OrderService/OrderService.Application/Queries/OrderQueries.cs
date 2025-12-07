using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Commands;

namespace OrderService.Application.Queries;

/// <summary>
/// Query to retrieve an order by ID.
/// CQRS Query pattern with MediatR.
/// </summary>
public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;

/// <summary>
/// Handler for GetOrderQuery.
/// </summary>
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderQueryRepository _queryRepository;
    private readonly ILogger<GetOrderQueryHandler> _logger;

    public GetOrderQueryHandler(
        IOrderQueryRepository queryRepository,
        ILogger<GetOrderQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching order: {OrderId}", request.OrderId);
        return await _queryRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);
    }
}

/// <summary>
/// Query to retrieve all orders for a customer.
/// </summary>
public record GetCustomerOrdersQuery(
    Guid CustomerId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderDto>>;

/// <summary>
/// Handler for GetCustomerOrdersQuery.
/// </summary>
public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderQueryRepository _queryRepository;
    private readonly ILogger<GetCustomerOrdersQueryHandler> _logger;

    public GetCustomerOrdersQueryHandler(
        IOrderQueryRepository queryRepository,
        ILogger<GetCustomerOrdersQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching orders for customer: {CustomerId}", request.CustomerId);
        return await _queryRepository.GetOrdersByCustomerIdAsync(
            request.CustomerId,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );
    }
}

/// <summary>
/// Query to retrieve orders by status.
/// Useful for dashboards and monitoring.
/// </summary>
public record GetOrdersByStatusQuery(
    string Status,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderDto>>;

/// <summary>
/// Handler for GetOrdersByStatusQuery.
/// </summary>
public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, PagedResult<OrderDto>>
{
    private readonly IOrderQueryRepository _queryRepository;
    private readonly ILogger<GetOrdersByStatusQueryHandler> _logger;

    public GetOrdersByStatusQueryHandler(
        IOrderQueryRepository queryRepository,
        ILogger<GetOrdersByStatusQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching orders with status: {Status}", request.Status);
        return await _queryRepository.GetOrdersByStatusAsync(
            request.Status,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );
    }
}

/// <summary>
/// Query to get order statistics.
/// Example of read model optimization - dedicated statistics query.
/// </summary>
public record GetOrderStatisticsQuery : IRequest<OrderStatisticsDto>;

/// <summary>
/// Handler for GetOrderStatisticsQuery.
/// </summary>
public class GetOrderStatisticsQueryHandler : IRequestHandler<GetOrderStatisticsQuery, OrderStatisticsDto>
{
    private readonly IOrderQueryRepository _queryRepository;
    private readonly ILogger<GetOrderStatisticsQueryHandler> _logger;

    public GetOrderStatisticsQueryHandler(
        IOrderQueryRepository queryRepository,
        ILogger<GetOrderStatisticsQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<OrderStatisticsDto> Handle(GetOrderStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching order statistics");
        return await _queryRepository.GetStatisticsAsync(cancellationToken);
    }
}

// ============================================================================
// Interfaces and DTOs
// ============================================================================

/// <summary>
/// Query repository interface for read operations.
/// In a full CQRS implementation with event sourcing, this queries denormalized read models.
/// </summary>
public interface IOrderQueryRepository
{
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetOrdersByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<PagedResult<OrderDto>> GetOrdersByStatusAsync(
        string status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
