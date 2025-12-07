using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Application.DTOs;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for Order Service.
/// Configures the database schema and relationships.
/// </summary>
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderLineItem> OrderLineItems { get; set; } = null!;

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CustomerId)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.CompletedAt);

            // Configure one-to-many relationship with OrderLineItem
            entity.HasMany(o => o.LineItems)
                .WithOne()
                .HasForeignKey(li => li.OrderId);

            // Indexes
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure OrderLineItem entity
        modelBuilder.Entity<OrderLineItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ProductId)
                .IsRequired();

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.OrderId)
                .IsRequired();

            entity.HasIndex(e => e.ProductId);
        });
    }
}

/// <summary>
/// Order repository implementation using Entity Framework Core.
/// Implements the repository pattern for data access abstraction.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(OrderDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching order: {OrderId}", id);
        return await _context.Orders
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding order: {OrderId}", order.Id);
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        _logger.LogDebug("Updating order: {OrderId}", order.Id);
        _context.Orders.Update(order);
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

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Order query repository implementation for optimized read operations.
/// Implements CQRS read model pattern.
/// </summary>
public class OrderQueryRepository : IOrderQueryRepository
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderQueryRepository> _logger;

    public OrderQueryRepository(OrderDbContext context, ILogger<OrderQueryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying order by ID: {OrderId}", orderId);
        
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        
        return order == null ? null : MapToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetOrdersByCustomerIdAsync(
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying orders for customer: {CustomerId}", customerId);
        
        var query = _context.Orders.AsNoTracking()
            .Where(o => o.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(o => o.LineItems)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<OrderDto>> GetOrdersByStatusAsync(
        string status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying orders by status: {Status}", status);
        
        if (!Enum.TryParse<OrderStatus>(status, out var orderStatus))
            throw new ArgumentException($"Invalid order status: {status}");

        var query = _context.Orders.AsNoTracking()
            .Where(o => o.Status == orderStatus);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(o => o.LineItems)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying order statistics");
        
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.LineItems)
            .ToListAsync(cancellationToken);

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var pendingOrders = orders.Where(o => o.Status == OrderStatus.Pending).ToList();
        var canceledOrders = orders.Where(o => o.Status == OrderStatus.Canceled).ToList();

        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var averageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;

        return new OrderStatisticsDto
        {
            TotalOrders = orders.Count,
            TotalRevenue = totalRevenue,
            CompletedOrders = completedOrders.Count,
            PendingOrders = pendingOrders.Count,
            CanceledOrders = canceledOrders.Count,
            AverageOrderValue = averageOrderValue
        };
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        CompletedAt = order.CompletedAt,
        LineItems = order.LineItems.Select(li => new OrderLineItemDto
        {
            ProductId = li.ProductId,
            ProductName = li.ProductName,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice
        }).ToList()
    };
}
