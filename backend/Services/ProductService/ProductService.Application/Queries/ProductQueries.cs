using MediatR;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs;

namespace ProductService.Application.Queries;

/// <summary>
/// Query to retrieve a product by ID.
/// Example of CQRS Query pattern with MediatR.
/// Queries don't modify state, so they're typically optimized for read performance.
/// </summary>
public record GetProductQuery(Guid Id) : IRequest<ProductDto?>;

/// <summary>
/// Handler for GetProductQuery.
/// Retrieves a product by ID from the read model.
/// In a real CQRS implementation with event sourcing, you could have a denormalized read model.
/// </summary>
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly IProductQueryRepository _queryRepository;
    private readonly ILogger<GetProductQueryHandler> _logger;

    public GetProductQueryHandler(
        IProductQueryRepository queryRepository,
        ILogger<GetProductQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<ProductDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product: {ProductId}", request.Id);
        var product = await _queryRepository.GetProductByIdAsync(request.Id, cancellationToken);
        return product;
    }
}

/// <summary>
/// Query to get all products with filtering and pagination.
/// </summary>
public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Category = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SearchTerm = null
) : IRequest<PagedResult<ProductDto>>;

/// <summary>
/// Handler for GetProductsQuery.
/// Implements filtering and pagination for optimal read performance.
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductQueryRepository _queryRepository;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        IProductQueryRepository queryRepository,
        ILogger<GetProductsQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching products with filters: Category={Category}, MinPrice={MinPrice}, MaxPrice={MaxPrice}",
            request.Category, request.MinPrice, request.MaxPrice);

        var result = await _queryRepository.GetProductsAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            category: request.Category,
            minPrice: request.MinPrice,
            maxPrice: request.MaxPrice,
            searchTerm: request.SearchTerm,
            cancellationToken: cancellationToken
        );

        return result;
    }
}

/// <summary>
/// Query to get products in a specific category.
/// </summary>
public record GetProductsByCategoryQuery(string Category) : IRequest<IEnumerable<ProductDto>>;

/// <summary>
/// Handler for GetProductsByCategoryQuery.
/// </summary>
public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, IEnumerable<ProductDto>>
{
    private readonly IProductQueryRepository _queryRepository;
    private readonly ILogger<GetProductsByCategoryQueryHandler> _logger;

    public GetProductsByCategoryQueryHandler(
        IProductQueryRepository queryRepository,
        ILogger<GetProductsByCategoryQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching products in category: {Category}", request.Category);
        return await _queryRepository.GetProductsByCategoryAsync(request.Category, cancellationToken);
    }
}

/// <summary>
/// Query to check inventory availability.
/// </summary>
public record CheckInventoryQuery(Guid ProductId, int Quantity) : IRequest<InventoryCheckDto>;

/// <summary>
/// Handler for CheckInventoryQuery.
/// </summary>
public class CheckInventoryQueryHandler : IRequestHandler<CheckInventoryQuery, InventoryCheckDto>
{
    private readonly IProductQueryRepository _queryRepository;
    private readonly ILogger<CheckInventoryQueryHandler> _logger;

    public CheckInventoryQueryHandler(
        IProductQueryRepository queryRepository,
        ILogger<CheckInventoryQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<InventoryCheckDto> Handle(CheckInventoryQuery request, CancellationToken cancellationToken)
    {
        var product = await _queryRepository.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new KeyNotFoundException($"Product {request.ProductId} not found");

        var isAvailable = product.StockQuantity >= request.Quantity;

        _logger.LogInformation(
            "Inventory check: ProductId={ProductId}, RequestedQuantity={Quantity}, Available={Available}",
            request.ProductId, request.Quantity, isAvailable);

        return new InventoryCheckDto
        {
            ProductId = request.ProductId,
            RequestedQuantity = request.Quantity,
            AvailableQuantity = product.StockQuantity,
            IsAvailable = isAvailable
        };
    }
}

// ============================================================================
// Interfaces and DTOs
// ============================================================================

/// <summary>
/// Query repository interface for read operations.
/// In a full CQRS implementation, this would query a denormalized read model.
/// </summary>
public interface IProductQueryRepository
{
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetProductsAsync(
        int pageNumber,
        int pageSize,
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default
    );
}

public class InventoryCheckDto
{
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
}
