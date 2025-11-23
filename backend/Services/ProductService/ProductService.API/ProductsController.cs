using Microsoft.AspNetCore.Mvc;
using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Queries;
using ProductService.Application.DTOs;

namespace ProductService.API.Controllers;

/// <summary>
/// Product API Controller.
/// Demonstrates REST endpoints for product management.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/products/{ProductId}", id);

        var query = new GetProductQuery(id);
        var product = await _mediator.Send(query, cancellationToken);

        if (product == null)
            return NotFound(new { message = $"Product {id} not found" });

        return Ok(product);
    }

    /// <summary>
    /// Get all products with optional filtering and pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="category">Filter by category</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="searchTerm">Search term in product name/description</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GET /api/v1/products?page={Page}&pageSize={PageSize}&category={Category}",
            pageNumber, pageSize, category);

        var query = new GetProductsQuery(pageNumber, pageSize, category, minPrice, maxPrice, searchTerm);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get products by category.
    /// </summary>
    /// <param name="category">Product category</param>
    /// <returns>Products in the category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(
        string category,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/products/category/{Category}", category);

        var query = new GetProductsByCategoryQuery(category);
        var products = await _mediator.Send(query, cancellationToken);

        return Ok(products);
    }

    /// <summary>
    /// Check inventory availability for a product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to check</param>
    /// <returns>Inventory availability information</returns>
    [HttpPost("inventory/check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryCheckDto>> CheckInventory(
        [FromQuery] Guid productId,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/products/inventory/check?productId={ProductId}&quantity={Quantity}", productId, quantity);

        var query = new CheckInventoryQuery(productId, quantity);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/products - Creating product: {ProductName}", request.Name);

        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.Sku,
            request.Category
        );

        try
        {
            var product = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid product data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update request</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/v1/products/{ProductId} - Updating product", id);

        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Category);

        try
        {
            var product = await _mediator.Send(command, cancellationToken);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Product {id} not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reserve inventory for an order.
    /// Called internally by OrderService via API Gateway.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <returns>Success status</returns>
    [HttpPost("inventory/reserve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> ReserveInventory(
        [FromQuery] Guid productId,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/v1/products/inventory/reserve?productId={ProductId}&quantity={Quantity}", productId, quantity);

        var command = new ReserveInventoryCommand(productId, quantity);

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Product {productId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Request/Response DTOs
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
