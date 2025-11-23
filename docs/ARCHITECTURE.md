# E-Commerce Microservices Architecture

## Overview

This is a production-grade, beginner-friendly e-commerce application built with microservices architecture. It demonstrates enterprise-level patterns and practices suitable for real-world applications.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Angular Frontend (NgRx)                      │
│                    - Product Catalog & Shopping Cart                 │
│                    - Order Management & History                      │
└─────────────────┬───────────────────────────────────────────────────┘
                  │ HTTP/REST
                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    API Gateway (YARP)                                │
│         - Request Routing & Load Balancing                          │
│         - Authentication & Authorization                            │
│         - Rate Limiting & Circuit Breaking                          │
└──┬──────────────────┬──────────────────┬───────────────────────────┘
   │                  │                  │
   ▼                  ▼                  ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│   Product    │  │    Order     │  │   Payment    │
│  Microservice│  │ Microservice │  │ Microservice │
│              │  │              │  │              │
│ Clean Arch   │  │ Clean Arch   │  │ Clean Arch   │
│ CQRS/MediatR │  │ CQRS/MediatR │  │ CQRS/MediatR │
│ EF Core+SQL  │  │ EF Core+SQL  │  │ EF Core+SQL  │
└────────┬─────┘  └────────┬─────┘  └────────┬─────┘
         │                 │                 │
         └─────────────┬───┴─────────────┬──┘
                       │
                       ▼
         ┌─────────────────────────────┐
         │  Azure Service Bus          │
         │  - Event Publishing         │
         │  - Async Message Queue      │
         │  - Topic/Subscriptions      │
         └──────────────┬──────────────┘
                        │
         ┌──────────────┼──────────────┐
         ▼              ▼              ▼
    ┌──────────┐  ┌──────────┐  ┌──────────┐
    │ Function │  │ Function │  │ Function │
    │  (Event  │  │ (Email   │  │ (Analytics)
    │ Consumer)│  │ Notifier)│  │          │
    └──────────┘  └──────────┘  └──────────┘
```

## Key Technologies

### Frontend
- **Angular**: Modern SPA framework
- **NgRx**: Predictable state management
- **RxJS**: Reactive programming
- **HttpClient**: API communication

### Backend Services (Each Service includes)
- **Clean Architecture**: Domain → Application → Infrastructure → API layers
- **.NET Core 8+**: Modern runtime
- **CQRS + MediatR**: Separates read/write operations
- **EF Core**: ORM with migrations
- **SQL Server**: Relational database
- **AutoMapper**: Object mapping

### Communication
- **Azure Service Bus**: Async messaging
- **Event Publishing**: Pub/Sub pattern
- **Azure Functions**: Serverless event processors

### API Gateway
- **YARP**: Modern reverse proxy
- **Ocelot**: (Alternative - simpler option)
- **Azure API Management**: (Alternative - fully managed)

## Microservices

### 1. Product Service
**Responsibility**: Product catalog management
- List/Search products
- Product details
- Inventory management
- Price updates

**Events Published**:
- `ProductCreated`
- `ProductUpdated`
- `InventoryReserved`
- `InventoryReleased`

### 2. Order Service
**Responsibility**: Order processing
- Create orders
- Order tracking
- Order history
- Order status management

**Events Published**:
- `OrderCreated`
- `OrderConfirmed`
- `OrderShipped`
- `OrderCanceled`

**Events Subscribed**:
- `PaymentProcessed`
- `ProductInventoryReserved`

### 3. Payment Service (Optional Expansion)
**Responsibility**: Payment processing
- Payment validation
- Transaction management
- Refund handling

**Events Published**:
- `PaymentProcessed`
- `PaymentFailed`
- `RefundIssued`

## Design Patterns Used

### 1. CQRS (Command Query Responsibility Segregation)
**Purpose**: Separate read and write operations
- **Commands**: Modify state (CreateProductCommand, PlaceOrderCommand)
- **Queries**: Retrieve state (GetProductQuery, GetOrdersQuery)
- **Benefits**: Scalability, optimization, cleaner code

### 2. MediatR Pattern
**Purpose**: In-process messaging (mediator pattern)
```
Handler sends Command/Query → MediatR → Handlers process → Return Result
```
- Decouples handlers
- Enables pipeline behaviors (logging, validation, transactions)
- Single Responsibility Principle

### 3. Clean Architecture
**Layers** (inside-out dependencies):
1. **Domain Layer**: Entities, Value Objects, Domain Rules
2. **Application Layer**: Use Cases, CQRS, DTOs
3. **Infrastructure Layer**: EF Core, Repositories, Messaging, External APIs
4. **API Layer**: Controllers, Middleware, Configuration

### 4. Repository Pattern
**Purpose**: Abstract data access
- Dependency inversion (depend on abstractions)
- Easy testing with mocks
- Flexible data source changes

### 5. Pub/Sub Messaging
**Purpose**: Loose coupling between services
- Event published to Service Bus
- Multiple subscribers can process asynchronously
- Service Bus ensures delivery

### 6. Saga Pattern (for complex workflows)
**Purpose**: Distributed transactions
- Order → Reserve Inventory → Process Payment → Create Shipment
- Compensation: Rollback if any step fails

## Data Flow Examples

### Creating an Order (Happy Path)
```
1. Client → API Gateway → Order Service
2. Order Service validates & creates order (Command)
3. Order Service publishes "OrderCreated" event to Service Bus
4. Event processors subscribe:
   - Email Notifier: Send confirmation email
   - Product Service: Reserve inventory
   - Analytics: Log order metrics
5. Product Service publishes "InventoryReserved" event
6. Order Service confirms order (Event Handler subscribes)
```

### Product Catalog Load (Read-Heavy)
```
1. Client → API Gateway → Product Service (Query)
2. Product Service queries read-optimized database
3. Returns cached results if available
4. Uses NgRx store in client for state
```

## Security Considerations

1. **API Gateway**: 
   - JWT validation
   - OAuth2/OpenID Connect integration
   - Rate limiting per client

2. **Service-to-Service**:
   - mTLS certificates
   - Service Bus connection strings (environment variables)
   - API key validation

3. **Data**:
   - Encryption at rest (SQL Server TDE)
   - Encryption in transit (HTTPS/TLS)
   - Sensitive data masking in logs

4. **Authentication**:
   - Azure AD integration
   - JWT tokens with refresh tokens
   - RBAC (Role-Based Access Control)

## Scalability & Resilience

1. **Horizontal Scaling**:
   - Stateless services (easy to scale)
   - Load balancer in front of each service
   - Database read replicas for read-heavy operations

2. **Resilience Patterns**:
   - Circuit Breaker (Polly library)
   - Retry with exponential backoff
   - Bulkhead isolation
   - Timeouts

3. **Message Queue**:
   - Azure Service Bus handles peak loads
   - Automatic scaling
   - Dead letter queues for failures

## Deployment Strategy

### Development
```
Local: Docker Compose with SQL Server, Service Bus emulator
```

### Staging/Production
```
Azure Container Instances (ACI) or AKS (Kubernetes)
- Each service in separate container
- API Gateway as entry point
- Managed SQL Server
- Azure Service Bus (standard tier)
- Azure Functions for event processing
```

## Monitoring & Observability

1. **Logging**: Application Insights, Serilog
2. **Tracing**: Distributed tracing (Application Insights)
3. **Metrics**: Custom metrics, performance counters
4. **Alerting**: Alert on error rates, latency, queue depth

## Development Workflow

1. **Local Development**:
   ```powershell
   docker-compose up  # Starts SQL Server, Service Bus emulator
   dotnet run         # Each service
   npm start          # Angular
   ```

2. **Testing**:
   - Unit tests (xUnit/NUnit)
   - Integration tests (TestContainers)
   - E2E tests (Cypress/Playwright)

3. **CI/CD**:
   - GitHub Actions / Azure DevOps
   - Build, test, containerize
   - Deploy to Azure

## Best Practices

✅ **Do**:
- Keep services small & focused (single responsibility)
- Use async messaging for inter-service communication
- Implement idempotency in event handlers
- Version APIs (`/api/v1/...`)
- Document with OpenAPI/Swagger
- Use correlation IDs for tracing

❌ **Don't**:
- Share databases between services
- Use synchronous HTTP between services (use messaging)
- Deploy monoliths as microservices
- Skip logging & monitoring
- Ignore database migrations

## Getting Started

1. Clone repository
2. Configure `appsettings.json` with connection strings
3. Run database migrations: `dotnet ef database update`
4. Start services in order: API Gateway, Product Service, Order Service
5. Access Angular app at `http://localhost:4200`

## References

- Martin Fowler on CQRS
- Clean Architecture (Robert C. Martin)
- Microservices Patterns (Chris Richardson)
- Azure Service Bus Documentation
- MediatR Documentation
