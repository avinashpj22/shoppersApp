# Quick Reference & Cheat Sheet

## Commands

### .NET CLI

```bash
# Create new project
dotnet new classlib -n ProjectName
dotnet new webapi -n ApiName

# Build & Run
dotnet build
dotnet run

# Database migrations
dotnet ef migrations add MigrationName
dotnet ef migrations remove
dotnet ef database update
dotnet ef database update 0  # Rollback all

# Testing
dotnet test
dotnet test --no-build --filter "ClassName"

# Package management
dotnet add package PackageName
dotnet remove package PackageName
dotnet list package
```

### Angular CLI

```bash
# Create new project
ng new project-name
ng generate component component-name
ng generate service service-name
ng generate module module-name

# Build & Run
ng serve
ng build --configuration production

# Testing
ng test
ng e2e

# Generate store with NgRx
ng generate @ngrx/schematics:action feature/product
ng generate @ngrx/schematics:reducer feature/product
ng generate @ngrx/schematics:effect feature/product
```

### Docker

```bash
# Build image
docker build -t image-name:tag .
docker build -f Dockerfile -t image-name .

# Run container
docker run -d -p 8000:8000 --name container-name image-name
docker run -it -v ${pwd}:/app image-name bash

# Manage containers
docker ps
docker logs container-name
docker stop container-name
docker rm container-name

# Compose
docker-compose up -d
docker-compose down
docker-compose logs -f service-name
```

### Git Workflow

```bash
# Setup branch
git checkout -b feature/product-management
git push -u origin feature/product-management

# Commit changes
git add .
git commit -m "feat: add product search functionality"
git push

# Create PR, review, merge
git checkout main
git pull
git merge feature/product-management
git push
```

---

## File Templates

### Product Service Controller

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id, CancellationToken ct)
    {
        var query = new GetProductQuery(id);
        var result = await _mediator.Send(query, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand(...);
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
```

### Command Handler

```csharp
public record MyCommand(...) : IRequest<MyDto>;

public class MyCommandHandler : IRequestHandler<MyCommand, MyDto>
{
    public async Task<MyDto> Handle(MyCommand request, CancellationToken ct)
    {
        // 1. Retrieve aggregate
        // 2. Apply business logic
        // 3. Persist changes
        // 4. Publish events
        // 5. Return DTO
    }
}
```

### Query Handler

```csharp
public record MyQuery(...) : IRequest<MyDto>;

public class MyQueryHandler : IRequestHandler<MyQuery, MyDto>
{
    private readonly IQueryRepository _repository;

    public async Task<MyDto> Handle(MyQuery request, CancellationToken ct)
    {
        return await _repository.GetAsync(request.Id, ct);
    }
}
```

### Angular Component with NgRx

```typescript
import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  products$: Observable<Product[]>;
  loading$: Observable<boolean>;

  constructor(private store: Store) {
    this.products$ = this.store.select(selectAllProducts);
    this.loading$ = this.store.select(selectProductsLoading);
  }

  ngOnInit() {
    this.store.dispatch(loadProducts({ pageNumber: 1, pageSize: 10 }));
  }
}
```

---

## Common Dependencies

### Backend

```xml
<!-- Domain -->
<PackageReference Include="MediatR" Version="12.0.0" />

<!-- Application -->
<PackageReference Include="MediatR" Version="12.0.0" />
<PackageReference Include="AutoMapper" Version="12.0.0" />
<PackageReference Include="FluentValidation" Version="11.0.0" />

<!-- Infrastructure -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.0.0" />

<!-- API -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Core" Version="2.2.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.0.0" />
<PackageReference Include="Serilog" Version="3.0.0" />
```

### Frontend

```json
{
  "dependencies": {
    "@angular/common": "^17.0.0",
    "@angular/core": "^17.0.0",
    "@ngrx/store": "^17.0.0",
    "@ngrx/effects": "^17.0.0",
    "@ngrx/store-devtools": "^17.0.0",
    "rxjs": "^7.8.0"
  }
}
```

---

## Naming Conventions

### C#
- Classes: `PascalCase` (ProductService, OrderRepository)
- Methods: `PascalCase` (GetProductAsync)
- Properties: `PascalCase` (ProductName)
- Private fields: `_camelCase` (_logger, _repository)
- Constants: `UPPER_SNAKE_CASE` (MAX_TIMEOUT)
- Interfaces: `IPascalCase` (IProductRepository)
- Commands: `VerbNounCommand` (CreateProductCommand)
- Queries: `VerbNounQuery` (GetProductQuery)
- Events: `VerbNounEvent` (ProductCreatedEvent)

### TypeScript/Angular
- Files: `kebab-case` (product.component.ts)
- Classes: `PascalCase` (ProductComponent)
- Methods: `camelCase` (getProducts)
- Constants: `UPPER_SNAKE_CASE` (API_URL)
- Selectors: `select` prefix (selectAllProducts)
- Actions: `verb noun` (loadProducts, loadProductsSuccess)
- Effects: `effect$` suffix (loadProducts$)
- Reducers: `reducer` prefix (productReducer)

---

## Git Commit Messages

```
feat: add product search functionality
fix: correct inventory calculation bug
docs: update API documentation
style: format code according to style guide
refactor: extract product validation to separate class
test: add unit tests for product creation
chore: update dependencies
perf: optimize database queries with indexes
```

Format: `<type>: <subject>`

---

## Status Codes Quick Reference

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK | Product retrieved successfully |
| 201 | Created | New product created |
| 204 | No Content | Delete successful |
| 400 | Bad Request | Invalid product data |
| 401 | Unauthorized | Missing JWT token |
| 403 | Forbidden | User can't access resource |
| 404 | Not Found | Product ID doesn't exist |
| 409 | Conflict | Duplicate SKU |
| 500 | Server Error | Database connection failed |

---

## Environment Variables

```bash
# .env.development
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=ProductDb;...
ConnectionStrings__ServiceBusConnection=Endpoint=sb://localhost:5672/;...
JWT_SECRET=your-secret-key
LOG_LEVEL=Debug

# .env.production
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=prod-server;Database=ProductDb;...
ConnectionStrings__ServiceBusConnection=Endpoint=sb://prod-namespace.servicebus.windows.net/...
JWT_SECRET=${Key Vault Reference}
LOG_LEVEL=Information
```

---

## Port Mappings

| Service | Port | Protocol |
|---------|------|----------|
| API Gateway | 8000 | HTTP |
| Product Service | 5000 | HTTP |
| Order Service | 5001 | HTTP |
| Frontend | 4200 | HTTP |
| SQL Server | 1433 | TCP |
| Service Bus | 5672 | AMQP |
| Azure Functions | 7071 | HTTP |

---

## Database Connection Strings

### SQL Server (Local)
```
Server=localhost;Database=ProductDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;
```

### SQL Server (Docker)
```
Server=sqlserver:1433;Database=ProductDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;
```

### SQL Server (Azure)
```
Server=tcp:server-name.database.windows.net,1433;Initial Catalog=DbName;Persist Security Info=False;User ID=admin;Password=YourPassword@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

## Useful Links

- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [Angular Docs](https://angular.io/docs)
- [NgRx Documentation](https://ngrx.io)
- [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)

