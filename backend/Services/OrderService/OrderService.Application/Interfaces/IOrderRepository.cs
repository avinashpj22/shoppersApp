using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Repository interface for Order aggregate.
/// Handles persistence operations for orders.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
