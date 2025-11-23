# E-Commerce Microservices Platform

A **production-grade, beginner-friendly** e-commerce application demonstrating enterprise microservices architecture with event-driven design.

## 🎯 Project Overview

This project showcases **best practices** for building scalable, maintainable microservices using:

- **Backend**: .NET Core with Clean Architecture + CQRS + MediatR
- **Frontend**: Angular with NgRx state management
- **Messaging**: Azure Service Bus for async communication
- **Database**: SQL Server with Entity Framework Core
- **API Gateway**: YARP for routing and load balancing
- **Cloud**: Azure (AKS, Functions, Service Bus, SQL Server)

## 📋 Table of Contents

- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Key Features](#key-features)
- [Development](#development)
- [Deployment](#deployment)
- [Documentation](#documentation)

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker Desktop
- SQL Server 2019+ (or Docker)
- Azure CLI (optional, for cloud deployment)

### Local Development (Docker Compose)

```bash
# Clone the repository
git clone <repository-url>
cd shoppers

# Start all services
docker-compose up -d

# Wait for services to be ready
docker-compose logs -f

# Access applications
# Frontend: http://localhost:4200
# API Gateway: http://localhost:8000
# Product Service: http://localhost:5000
# Order Service: http://localhost:5001
```

### Manual Setup

**Terminal 1 - Product Service:**
```bash
cd backend/Services/ProductService/ProductService.API
dotnet run
```

**Terminal 2 - Order Service:**
```bash
cd backend/Services/OrderService/OrderService.API
dotnet run
```

**Terminal 3 - API Gateway:**
```bash
cd backend/Gateway/ApiGateway
dotnet run
```

**Terminal 4 - Frontend:**
```bash
cd frontend
npm install
npm start
```

## 🏗️ Architecture

### Microservices Pattern

```
Client (Angular)
    ↓
API Gateway (YARP)
    ├─→ Product Service
    └─→ Order Service
         ↓
    Shared Resources
    ├─→ SQL Server (Product DB, Order DB)
    ├─→ Azure Service Bus (Events)
    └─→ Azure Functions (Event Processors)
```

### Event Flow

```
User places order
    ↓
OrderService creates order → publishes "OrderCreated" event
    ↓
Event published to Service Bus
    ↓
Multiple subscribers process:
    1. Email Service → sends confirmation
    2. Product Service → reserves inventory
    3. Analytics → logs metrics
    4. Payment Service → processes payment
```

### Design Patterns

| Pattern | Purpose | Implementation |
|---------|---------|-----------------|
| **CQRS** | Separate read/write | Commands & Queries in MediatR |
| **Event Sourcing** | Event-driven | Domain events published to Service Bus |
| **Repository** | Data abstraction | IProductRepository interface |
| **Saga** | Distributed transactions | Order workflow coordination |
| **Circuit Breaker** | Fault tolerance | Polly library integration |
| **API Gateway** | Single entry point | YARP reverse proxy |

## 📚 Technology Stack

### Backend
- **.NET 8** - Modern runtime and framework
- **Entity Framework Core** - ORM with migrations
- **MediatR** - Mediator pattern for CQRS
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Polly** - Resilience and retry policies
- **xUnit** - Unit testing framework
- **Moq** - Mocking library

### Frontend
- **Angular 17** - Component framework
- **NgRx** - Predictable state management
- **RxJS** - Reactive programming
- **TypeScript** - Type-safe JavaScript
- **Angular Material** - UI components

### Cloud & Infrastructure
- **Azure Service Bus** - Message broker
- **Azure SQL Server** - Relational database
- **Azure Functions** - Event processors
- **Azure App Service** - Application hosting
- **Azure Container Registry** - Docker image storage
- **Azure Kubernetes Service (AKS)** - Container orchestration

### Tools
- **Docker** - Containerization
- **Docker Compose** - Local orchestration
- **YARP** - API Gateway
- **Azure CLI** - Cloud management
- **Git** - Version control

## 📁 Project Structure

```
shoppers/
├── backend/
│   ├── Gateway/
│   │   └── ApiGateway/              # YARP API Gateway configuration
│   ├── Services/
│   │   ├── ProductService/
│   │   │   ├── ProductService.Domain/        # Business logic, entities
│   │   │   ├── ProductService.Application/   # CQRS handlers, DTOs
│   │   │   ├── ProductService.Infrastructure/# EF Core, repositories, messaging
│   │   │   └── ProductService.API/          # Controllers, endpoints
│   │   └── OrderService/                     # Similar structure
│   └── Workers/
│       └── EventProcessors/            # Azure Functions event handlers
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── models/              # TypeScript interfaces
│   │   │   ├── services/            # API services
│   │   │   ├── store/               # NgRx actions, reducers, effects
│   │   │   ├── features/            # Feature modules
│   │   │   │   ├── products/        # Product feature
│   │   │   │   └── orders/          # Orders feature
│   │   │   └── guards/              # Route guards
│   │   └── environments/            # Environment configurations
│   ├── angular.json
│   └── package.json
├── docs/
│   ├── README.md                     # This file
│   ├── ARCHITECTURE.md              # Detailed architecture
│   ├── API_SPECIFICATION.md        # API documentation
│   ├── DEPLOYMENT.md               # Deployment guide
│   ├── DEVELOPMENT.md              # Development guide
│   └── QUICK_REFERENCE.md          # Commands & cheat sheets
└── docker-compose.yml              # Local development environment
```

## ✨ Key Features

### Product Management
- ✅ Browse product catalog with pagination
- ✅ Filter by category, price range
- ✅ Search products
- ✅ Check inventory availability
- ✅ Admin product CRUD operations

### Order Management
- ✅ Place orders with shopping cart
- ✅ Track order status in real-time
- ✅ Order history and details
- ✅ Order cancellation (before shipment)
- ✅ Inventory reservation on order creation

### Event Processing
- ✅ Asynchronous event publishing
- ✅ Email notifications (order confirmation, shipment)
- ✅ Analytics tracking
- ✅ Automatic inventory management
- ✅ Payment integration

### API Features
- ✅ RESTful API with API versioning
- ✅ Pagination and filtering
- ✅ JWT authentication
- ✅ OpenAPI/Swagger documentation
- ✅ Request correlation IDs
- ✅ Structured logging

### Frontend Features
- ✅ Responsive design
- ✅ Product search and filtering
- ✅ Shopping cart management
- ✅ NgRx state management
- ✅ Loading states and error handling
- ✅ Reactive forms

## 💻 Development

### Creating a New Feature

1. **Design the domain model**
   ```csharp
   Domain/Entities/MyEntity.cs
   ```

2. **Create CQRS handlers**
   ```csharp
   Application/Commands/MyCommand.cs
   Application/Queries/MyQuery.cs
   ```

3. **Implement API endpoint**
   ```csharp
   API/Controllers/MyController.cs
   ```

4. **Create tests**
   ```csharp
   Tests/MyFeatureTests.cs
   ```

See [DEVELOPMENT.md](docs/DEVELOPMENT.md) for detailed guide.

### Running Tests

```bash
# Backend unit tests
dotnet test

# Backend with coverage
dotnet test /p:CollectCoverage=true

# Frontend tests
npm test
```

## 🚢 Deployment

### Local Development
```bash
docker-compose up -d
```

### Azure Deployment
```bash
# Create resources
az group create --name ecommerce-rg --location eastus

# Build and push images
az acr build --registry ecommerceacr --image ecommerce/api-gateway:1.0 .

# Deploy to AKS
kubectl apply -f kubernetes/deployment.yaml
```

See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for complete guide.

## � Documentation

For detailed reference docs, see the `docs/` folder:
- **[ARCHITECTURE.md](docs/ARCHITECTURE.md)** - System design, patterns
- **[API_SPECIFICATION.md](docs/API_SPECIFICATION.md)** - API documentation
- **[DEPLOYMENT.md](docs/DEPLOYMENT.md)** - Deployment guide
- **[DEVELOPMENT.md](docs/DEVELOPMENT.md)** - Development workflow
- **[QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md)** - Commands & cheat sheet

## 🔍 API Examples

### Get Products
```bash
curl -X GET "http://localhost:8000/api/v1/products?pageNumber=1&pageSize=10&category=Electronics" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Create Product
```bash
curl -X POST "http://localhost:8000/api/v1/products" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop Pro",
    "description": "High-performance laptop",
    "price": 1299.99,
    "stockQuantity": 15,
    "sku": "LAPTOP-PRO-001",
    "category": "Electronics"
  }'
```

### Place Order
```bash
curl -X POST "http://localhost:8000/api/v1/orders" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "770e8400-e29b-41d4-a716-446655440000",
    "lineItems": [
      {
        "productId": "550e8400-e29b-41d4-a716-446655440000",
        "productName": "Laptop Pro",
        "quantity": 1,
        "unitPrice": 1299.99
      }
    ]
  }'
```

See [API_SPECIFICATION.md](docs/API_SPECIFICATION.md) for complete API reference.

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Port already in use | Change port in appsettings.json or kill process |
| Database connection error | Check SQL Server is running, verify connection string |
| Service Bus errors | Verify emulator is running, check connection string |
| Angular compilation errors | Delete node_modules, run `npm install` |
| Docker containers failing | Check logs: `docker-compose logs service-name` |

## 📚 Learning Resources

- [Microsoft .NET Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/)
- [CQRS Pattern by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Angular Guide](https://angular.io/guide)
- [NgRx Documentation](https://ngrx.io)
- [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)

## 🤝 Contributing

1. Create a feature branch: `git checkout -b feature/amazing-feature`
2. Commit changes: `git commit -m 'feat: add amazing feature'`
3. Push to branch: `git push origin feature/amazing-feature`
4. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💼 Support

For issues, questions, or suggestions:
1. Check [TROUBLESHOOTING](docs/QUICK_REFERENCE.md#troubleshooting)
2. Review [DEVELOPMENT.md](docs/DEVELOPMENT.md) for common tasks
3. Open an issue on GitHub

---

## 📊 Project Statistics

- **Microservices**: 2 (Product, Order)
- **API Endpoints**: 20+
- **Database Tables**: 6+
- **Domain Events**: 8+
- **Azure Functions**: 4+
- **Lines of Code**: 3000+
- **Documentation Pages**: 5+

## 🎓 Educational Value

This project demonstrates:

✅ **Enterprise Patterns**: CQRS, Event Sourcing, Repository, Mediator  
✅ **Cloud-Native Design**: Microservices, Azure services, containers  
✅ **Best Practices**: Clean code, testing, logging, security  
✅ **Modern Tech**: .NET 8, Angular, TypeScript, Docker, Kubernetes  
✅ **DevOps**: CI/CD, containerization, cloud deployment  

Perfect for learning and implementing production-grade applications!

---

**Created**: November 2024  
**Last Updated**: November 23, 2025  
**Version**: 1.0.0

