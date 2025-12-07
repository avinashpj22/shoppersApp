using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

/// <summary>
/// Repository interface for Product aggregate.
/// Handles persistence operations for products.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
