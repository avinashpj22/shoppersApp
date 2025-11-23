# Development Guide & Best Practices

## Project Structure Overview

```
shoppers/
├── backend/
│   ├── Gateway/
│   │   └── ApiGateway/                 # YARP API Gateway
│   ├── Services/
│   │   ├── ProductService/
│   │   │   ├── ProductService.Domain/        # Entities, DDD
│   │   │   ├── ProductService.Application/   # CQRS, Use Cases
│   │   │   ├── ProductService.Infrastructure/# EF Core, Messaging
│   │   │   └── ProductService.API/          # Controllers, DI
│   │   └── OrderService/                     # Similar structure
│   └── Workers/
│       └── EventProcessors/            # Azure Functions
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── models/                # TypeScript interfaces
│   │   │   ├── services/              # HTTP services
│   │   │   ├── store/                 # NgRx state management
│   │   │   ├── features/              # Feature modules
│   │   │   └── guards/                # Route guards
│   │   └── environments/
│   └── angular.json
└── docs/
    ├── ARCHITECTURE.md                 # System design
    ├── API_SPECIFICATION.md           # API docs
    ├── DEPLOYMENT.md                  # Deployment guide
    └── DEVELOPMENT.md                 # This file
```

## Architecture Patterns

### 1. Clean Architecture

**Layer Responsibilities:**

| Layer | Purpose | Dependencies |
|-------|---------|--------------|
| **API** | Controllers, routes, serialization | Application |
| **Application** | Commands, Queries, Handlers, DTOs | Domain, Infrastructure interfaces |
| **Infrastructure** | EF Core, Repository, Messaging | Domain |
| **Domain** | Entities, Business logic, Aggregates | None (except System) |

**Key Principle**: Dependencies point inward. Outer layers can depend on inner layers, never the opposite.

### 2. CQRS Pattern

**Separates:**
- **Commands**: Operations that modify state
- **Queries**: Operations that read state

**Benefits:**
- Scalable: Read and write models can scale independently
- Optimizable: Read model can be denormalized
- Clear intent: Immediately obvious what each operation does

### 3. Event-Driven Architecture

**Flow:**
1. Aggregate root publishes domain events
2. Infrastructure publishes to Service Bus
3. Other services subscribe and react
4. Maintains loose coupling

### 4. Repository Pattern

**Isolates data access:**
```csharp
// Domain layer (interface)
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
}

// Infrastructure layer (implementation)
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;
    public async Task<Product?> GetByIdAsync(Guid id) => 
        await _context.Products.FindAsync(id);
}
```

## Development Workflow

### Creating a New Feature

#### 1. Define Domain Model

`Domain/Entities/YourEntity.cs`:
```csharp
public class YourEntity
{
    public Guid Id { get; private set; }
    // Properties...
    
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events;
    
    // Factory method
    public static YourEntity Create(...) => new() { ... };
    
    // Business methods
    public void DoSomething() { ... }
}
```

#### 2. Create CQRS Handlers

`Application/Commands/YourCommand.cs`:
```csharp
public record CreateYourEntityCommand(...) : IRequest<YourEntityDto>;

public class CreateYourEntityCommandHandler : IRequestHandler<CreateYourEntityCommand, YourEntityDto>
{
    public async Task<YourEntityDto> Handle(CreateYourEntityCommand request, CancellationToken ct)
    {
        var entity = YourEntity.Create(...);
        await _repository.AddAsync(entity, ct);
        foreach (var @event in entity.DomainEvents)
            await _eventPublisher.PublishAsync(@event, ct);
        return _mapper.Map<YourEntityDto>(entity);
    }
}
```

#### 3. Create Controller Endpoint

`API/Controllers/YourController.cs`:
```csharp
[HttpPost]
public async Task<ActionResult<YourEntityDto>> Create(
    CreateYourEntityRequest request,
    CancellationToken ct)
{
    var command = new CreateYourEntityCommand(...);
    var result = await _mediator.Send(command, ct);
    return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
}
```

#### 4. Test

```csharp
[Fact]
public async Task CreateYourEntity_ValidRequest_ReturnsCreatedResponse()
{
    // Arrange
    var command = new CreateYourEntityCommand(...);
    
    // Act
    var result = await _mediator.Send(command, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    Assert.NotEqual(Guid.Empty, result.Id);
}
```

## Common Tasks

### Adding a Database Migration

```bash
# Product Service
cd backend/Services/ProductService/ProductService.Infrastructure
dotnet ef migrations add AddNewColumn --context ProductDbContext
dotnet ef database update --context ProductDbContext
```

### Publishing an Event

Events are automatically published in command handlers:

```csharp
foreach (var @event in aggregate.DomainEvents)
{
    await _eventPublisher.PublishAsync(@event, cancellationToken);
}
aggregate.ClearDomainEvents();
```

### Subscribing to Events

In `Program.cs` or startup:

```csharp
var subscriber = serviceProvider.GetRequiredService<AzureServiceBusEventSubscriber>();
await subscriber.SubscribeAsync(
    topicName: "orders.events",
    subscriptionName: "my-service-subscription",
    handler: async (args) => {
        // Handle event
        await args.CompleteMessageAsync(args.CancellationToken);
    },
    errorHandler: async (args) => {
        _logger.LogError(args.Exception, "Error processing event");
    }
);
```

### Using NgRx Store in Angular

**Component:**
```typescript
export class ProductListComponent implements OnInit {
  products$ = this.store.select(selectAllProducts);
  loading$ = this.store.select(selectProductsLoading);
  error$ = this.store.select(selectProductsError);

  constructor(private store: Store) {}

  ngOnInit() {
    this.store.dispatch(loadProducts({
      pageNumber: 1,
      pageSize: 10,
      category: 'Electronics'
    }));
  }
}
```

**Template:**
```html
<div *ngIf="loading$ | async">Loading...</div>
<div *ngIf="error$ | async as error" class="alert">{{ error }}</div>
<div *ngFor="let product of (products$ | async)">
  {{ product.name }} - ${{ product.price }}
</div>
```

## Testing Strategy

### Unit Tests (xUnit + Moq)

```csharp
[Fact]
public async Task CreateProduct_WithValidData_Success()
{
    // Arrange
    var productName = "Test Product";
    var command = new CreateProductCommand(productName, ..., 99.99, 10, "SKU", "Test");
    
    var mockRepository = new Mock<IProductRepository>();
    var mockPublisher = new Mock<IEventPublisher>();
    var handler = new CreateProductCommandHandler(mockRepository.Object, mockPublisher.Object, Mock.Of<ILogger>());
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(productName, result.Name);
    mockRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Tests (TestContainers)

```csharp
[Fact]
public async Task GetProduct_WithValidId_ReturnsProduct()
{
    // Arrange - Uses real database via TestContainers
    var dbContainer = new MssqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithEnvironment("SA_PASSWORD", "YourPassword@123")
        .Build();
    
    await dbContainer.StartAsync();
    var connectionString = dbContainer.GetConnectionString();
    
    // Create DbContext and add test data
    var context = new ProductDbContext(...);
    await context.Database.MigrateAsync();
    
    // Act & Assert
}
```

### API Tests (xUnit + HttpClientFactory)

```csharp
[Fact]
public async Task GetProduct_ValidId_Returns200()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync($"/api/v1/products/{testProductId}");
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsAsync<ProductDto>();
    Assert.NotNull(content);
}
```

## Performance Optimization

### 1. Database Indexing

```csharp
entity.HasIndex(e => e.Category);
entity.HasIndex(e => e.CreatedAt);
entity.HasIndex(e => e.Sku).IsUnique();
```

### 2. Query Optimization

```csharp
// ❌ Bad: N+1 query
var products = await _context.Products.ToListAsync();
foreach (var product in products)
{
    var reviews = await _context.Reviews
        .Where(r => r.ProductId == product.Id)
        .ToListAsync(); // Called N times
}

// ✅ Good: Single query with Include
var products = await _context.Products
    .Include(p => p.Reviews)
    .ToListAsync();
```

### 3. Caching (Redis)

```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IDistributedCache _cache;
    
    public async Task<Product> GetProductAsync(Guid id)
    {
        var cached = await _cache.GetStringAsync($"product:{id}");
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<Product>(cached);
        
        var product = await _inner.GetProductAsync(id);
        await _cache.SetStringAsync($"product:{id}", 
            JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
        
        return product;
    }
}
```

### 4. Async/Await

```csharp
// ✅ Good: Proper async
public async Task<Product> GetProductAsync(Guid id)
{
    return await _context.Products.FindAsync(id);
}

// ❌ Bad: Sync over async
public Product GetProduct(Guid id)
{
    return _context.Products.FindAsync(id).Result; // Deadlock risk
}
```

## Security Best Practices

### 1. Input Validation

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
```

### 2. Authorization

```csharp
[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<ActionResult> CreateProduct(CreateProductRequest request)
{
    // Only admins can create products
}
```

### 3. Data Protection

```csharp
// Encrypt sensitive data
data.CreditCard = _dataProtector.Protect(data.CreditCard);

// Decrypt when needed
var decrypted = _dataProtector.Unprotect(data.CreditCard);
```

### 4. Secure Secrets

```csharp
// ❌ Bad
var connectionString = "Server=...;Password=hardcoded";

// ✅ Good
var connectionString = configuration.GetConnectionString("DefaultConnection");
// In Azure Key Vault, environment variables, or appsettings.json
```

## Debugging Tips

### Enable Detailed Logging

```csharp
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.EntityFrameworkCore.Database.Command": "Debug"
  }
}
```

### SQL Server Profiler

```sql
-- See all queries
SELECT * FROM sys.dm_exec_requests
WHERE session_id > 50
```

### Debug with Breakpoints

```csharp
System.Diagnostics.Debugger.Break(); // Pause execution
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| **Service Bus connection errors** | Check connection string, verify namespace exists |
| **EF Core migration conflicts** | Remove migrations, start fresh with `Remove-Migration` |
| **CORS errors in frontend** | Add CORS policy in API Gateway |
| **Slow queries** | Check indexes, use profiler, consider caching |
| **Out of memory** | Check for circular references, memory leaks in Azure Functions |

## Resources

- [Microsoft Docs: .NET Architecture](https://docs.microsoft.com/en-us/dotnet/architecture)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core)
- [Angular Documentation](https://angular.io/docs)
- [NgRx Documentation](https://ngrx.io)

