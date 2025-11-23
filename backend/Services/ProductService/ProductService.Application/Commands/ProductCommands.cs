using MediatR;
using Microsoft.Extensions.Logging;
using ProductService.Domain.Entities;
using ProductService.Application.DTOs;

namespace ProductService.Application.Commands;

/// <summary>
/// Command to create a new product.
/// Example of CQRS Command pattern with MediatR.
/// </summary>
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Sku,
    string Category
) : IRequest<ProductDto>;

/// <summary>
/// Handler for CreateProductCommand.
/// Implements the business logic for product creation.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IEventPublisher eventPublisher,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Domain logic (creating aggregate)
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.Sku,
            request.Category
        );

        // 2. Persistence
        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        // 3. Publish domain events to Service Bus
        foreach (var domainEvent in product.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        product.ClearDomainEvents();

        _logger.LogInformation("Product created: {ProductId}, Name: {ProductName}", product.Id, product.Name);

        // 4. Return DTO
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Sku = product.Sku,
            Category = product.Category,
            IsActive = product.IsActive
        };
    }
}

/// <summary>
/// Command to update an existing product.
/// </summary>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category
) : IRequest<ProductDto>;

/// <summary>
/// Handler for UpdateProductCommand.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IEventPublisher eventPublisher,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve aggregate
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found");

        // 2. Apply domain logic
        product.Update(request.Name, request.Description, request.Price, request.Category);

        // 3. Persist changes
        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        // 4. Publish events
        foreach (var domainEvent in product.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        product.ClearDomainEvents();

        _logger.LogInformation("Product updated: {ProductId}", product.Id);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Sku = product.Sku,
            Category = product.Category,
            IsActive = product.IsActive
        };
    }
}

/// <summary>
/// Command to reserve inventory for an order.
/// Published by OrderService, handled by ProductService.
/// </summary>
public record ReserveInventoryCommand(
    Guid ProductId,
    int Quantity
) : IRequest<bool>;

/// <summary>
/// Handler for ReserveInventoryCommand.
/// </summary>
public class ReserveInventoryCommandHandler : IRequestHandler<ReserveInventoryCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ReserveInventoryCommandHandler> _logger;

    public ReserveInventoryCommandHandler(
        IProductRepository productRepository,
        IEventPublisher eventPublisher,
        ILogger<ReserveInventoryCommandHandler> logger)
    {
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ReserveInventoryCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found");

        // Domain logic: reserve stock
        product.ReserveStock(request.Quantity);

        // Persist
        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        foreach (var domainEvent in product.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        product.ClearDomainEvents();

        _logger.LogInformation("Inventory reserved: ProductId={ProductId}, Quantity={Quantity}", 
            request.ProductId, request.Quantity);

        return true;
    }
}

// ============================================================================
// Interfaces and DTOs
// ============================================================================

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Sku { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; }
}
