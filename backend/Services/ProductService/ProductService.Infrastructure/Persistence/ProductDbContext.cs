using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Domain.Entities;
using ProductService.Application.DTOs;

namespace ProductService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for Product Service.
/// Configures the database schema and relationships.
/// </summary>
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Sku)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.Property(e => e.StockQuantity)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            // Create indexes for common queries
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}

/// <summary>
/// Product repository implementation using Entity Framework Core.
/// Implements the repository pattern for data access abstraction.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ProductDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching product: {ProductId}", id);
        return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding product: {ProductId}", product.Id);
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _logger.LogDebug("Updating product: {ProductId}", product.Id);
        _context.Products.Update(product);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Changes saved successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict while saving changes");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error");
            throw;
        }
    }
}

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Product query repository implementation for optimized read operations.
/// Implements CQRS read model pattern.
/// </summary>
public class ProductQueryRepository : IProductQueryRepository
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductQueryRepository> _logger;

    public ProductQueryRepository(ProductDbContext context, ILogger<ProductQueryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying product by ID: {ProductId}", id);
        
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        
        return product == null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(
        string category, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying products by category: {Category}", category);
        
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.Category == category && p.IsActive)
            .ToListAsync(cancellationToken);
        
        return products.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(
        int pageNumber,
        int pageSize,
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Querying products with filters: Category={Category}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, SearchTerm={SearchTerm}",
            category, minPrice, maxPrice, searchTerm);

        var query = _context.Products.AsNoTracking().Where(p => p.IsActive);

        // Apply filters
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice);

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.Description.Contains(searchTerm));

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static ProductDto MapToDto(Product product) => new()
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
