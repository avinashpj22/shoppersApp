using OrderService.Application.DTOs;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Order query repository interface for optimized read operations.
/// Implements CQRS read model pattern.
/// </summary>
public interface IOrderQueryRepository
{
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetOrdersByCustomerIdAsync(
        Guid customerId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetOrdersByStatusAsync(
        string status,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
