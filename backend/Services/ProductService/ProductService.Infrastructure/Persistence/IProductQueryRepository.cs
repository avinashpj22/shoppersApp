using ProductService.Application.DTOs;

namespace ProductService.Infrastructure.Persistence;

/// <summary>
/// Product query repository interface for optimized read operations.
/// Implements CQRS read model pattern.
/// </summary>
public interface IProductQueryRepository
{
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
