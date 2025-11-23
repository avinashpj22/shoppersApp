# Deployment & Infrastructure Guide

## Local Development Setup

### Prerequisites
- .NET 8 SDK
- SQL Server 2019+ or SQL Server Express
- Docker Desktop (for local Service Bus emulator)
- Node.js 18+ with npm
- Azure CLI
- Azure Storage Emulator / Azurite

### Step 1: Local Infrastructure

#### Start Azure Service Bus Emulator
```bash
# Using Docker
docker run -d `
  --name servicebus-emulator `
  -p 5672:5672 `
  mcr.microsoft.com/azure-messaging/servicebus-emulator:latest

# Or download from: https://github.com/Azure/azure-service-bus-emulator
```

#### SQL Server with Docker
```bash
docker run -e "ACCEPT_EULA=Y" `
  -e "SA_PASSWORD=YourPassword@123" `
  -p 1433:1433 `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

#### Connection Strings for Local Development
Create `appsettings.Development.json` in each service:

**ProductService:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;",
    "ServiceBusConnection": "Endpoint=sb://localhost:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Step 2: Database Setup

#### Create Databases
```bash
# Product Service
dotnet ef migrations add InitialCreate `
  --project ProductService.Infrastructure `
  --startup-project ProductService.API

dotnet ef database update `
  --project ProductService.Infrastructure `
  --startup-project ProductService.API

# Order Service
dotnet ef migrations add InitialCreate `
  --project OrderService.Infrastructure `
  --startup-project OrderService.API

dotnet ef database update `
  --project OrderService.Infrastructure `
  --startup-project OrderService.API
```

### Step 3: Run Services Locally

**Terminal 1 - API Gateway:**
```bash
cd backend/Gateway/ApiGateway
dotnet run
# Runs on http://localhost:8000
```

**Terminal 2 - Product Service:**
```bash
cd backend/Services/ProductService/ProductService.API
dotnet run
# Runs on http://localhost:5000
```

**Terminal 3 - Order Service:**
```bash
cd backend/Services/OrderService/OrderService.API
dotnet run
# Runs on http://localhost:5001
```

**Terminal 4 - Frontend:**
```bash
cd frontend
npm install
npm start
# Runs on http://localhost:4200
```

**Terminal 5 - Azure Functions (Event Processors):**
```bash
cd backend/Workers/EventProcessors
func start
```

## Docker Compose Setup (Recommended)

Create `docker-compose.yml` in root:

```yaml
version: '3.9'

services:
  # Database
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourPassword@123"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  # Service Bus Emulator
  servicebus:
    image: mcr.microsoft.com/azure-messaging/servicebus-emulator:latest
    ports:
      - "5672:5672"

  # Product Service
  product-service:
    build:
      context: ./backend/Services/ProductService
      dockerfile: ProductService.API/Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=ProductDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;"
      ConnectionStrings__ServiceBusConnection: "Endpoint=sb://servicebus:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;"
    ports:
      - "5000:5000"
    depends_on:
      - sqlserver
      - servicebus

  # Order Service
  order-service:
    build:
      context: ./backend/Services/OrderService
      dockerfile: OrderService.API/Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=OrderDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;"
      ConnectionStrings__ServiceBusConnection: "Endpoint=sb://servicebus:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;"
    ports:
      - "5001:5001"
    depends_on:
      - sqlserver
      - servicebus

  # API Gateway
  api-gateway:
    build:
      context: ./backend/Gateway
      dockerfile: ApiGateway/Dockerfile
    ports:
      - "8000:8000"
    depends_on:
      - product-service
      - order-service

  # Frontend
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - "4200:80"
    depends_on:
      - api-gateway

volumes:
  sqlserver_data:
```

**Run with Docker Compose:**
```bash
docker-compose up -d
```

## Production Deployment (Azure)

### Step 1: Create Azure Resources

```bash
# Set variables
$resourceGroup = "ecommerce-rg"
$location = "eastus"
$acr = "ecommerceacr"
$aksCluster = "ecommerce-aks"
$serviceBus = "ecommerce-sb"
$sqlServer = "ecommerce-sql"

# Create resource group
az group create `
  --name $resourceGroup `
  --location $location

# Create Container Registry
az acr create `
  --resource-group $resourceGroup `
  --name $acr `
  --sku Basic

# Create AKS Cluster
az aks create `
  --resource-group $resourceGroup `
  --name $aksCluster `
  --node-count 3 `
  --vm-set-type VirtualMachineScaleSets `
  --load-balancer-sku standard `
  --enable-managed-identity `
  --network-plugin azure

# Create Service Bus Namespace
az servicebus namespace create `
  --resource-group $resourceGroup `
  --name $serviceBus `
  --location $location `
  --sku Standard

# Create SQL Server
az sql server create `
  --resource-group $resourceGroup `
  --name $sqlServer `
  --location $location `
  --admin-user sqladmin `
  --admin-password 'P@ssw0rd!@#'

# Create SQL Databases
az sql db create `
  --resource-group $resourceGroup `
  --server $sqlServer `
  --name ProductDb

az sql db create `
  --resource-group $resourceGroup `
  --server $sqlServer `
  --name OrderDb
```

### Step 2: Build and Push Docker Images

```bash
# Login to ACR
az acr login --name $acr

# Build images
docker build -t ecommerce/product-service:1.0 `
  -f ./backend/Services/ProductService/ProductService.API/Dockerfile `
  ./backend/Services/ProductService

docker build -t ecommerce/order-service:1.0 `
  -f ./backend/Services/OrderService/OrderService.API/Dockerfile `
  ./backend/Services/OrderService

docker build -t ecommerce/api-gateway:1.0 `
  -f ./backend/Gateway/ApiGateway/Dockerfile `
  ./backend/Gateway

# Tag for ACR
docker tag ecommerce/product-service:1.0 `
  "$acr.azurecr.io/ecommerce/product-service:1.0"
docker tag ecommerce/order-service:1.0 `
  "$acr.azurecr.io/ecommerce/order-service:1.0"
docker tag ecommerce/api-gateway:1.0 `
  "$acr.azurecr.io/ecommerce/api-gateway:1.0"

# Push to ACR
docker push "$acr.azurecr.io/ecommerce/product-service:1.0"
docker push "$acr.azurecr.io/ecommerce/order-service:1.0"
docker push "$acr.azurecr.io/ecommerce/api-gateway:1.0"
```

### Step 3: Deploy to AKS

Create `kubernetes/deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: product-service
spec:
  replicas: 2
  selector:
    matchLabels:
      app: product-service
  template:
    metadata:
      labels:
        app: product-service
    spec:
      containers:
      - name: product-service
        image: ecommerceacr.azurecr.io/ecommerce/product-service:1.0
        ports:
        - containerPort: 5000
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secrets
              key: product-connection-string
        - name: ConnectionStrings__ServiceBusConnection
          valueFrom:
            secretKeyRef:
              name: servicebus-secrets
              key: connection-string

---
apiVersion: v1
kind: Service
metadata:
  name: product-service
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 5000
  selector:
    app: product-service
```

```bash
# Get AKS credentials
az aks get-credentials `
  --resource-group $resourceGroup `
  --name $aksCluster

# Create secrets
kubectl create secret generic db-secrets `
  --from-literal=product-connection-string='...' `
  --from-literal=order-connection-string='...'

kubectl create secret generic servicebus-secrets `
  --from-literal=connection-string='...'

# Deploy
kubectl apply -f kubernetes/deployment.yaml
```

## Monitoring & Logging

### Application Insights Setup

```bash
# Create Application Insights
az monitor app-insights component create `
  --app ecommerce-insights `
  --resource-group $resourceGroup `
  --location $location
```

### Add to appsettings.json

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

### Configure in Startup

```csharp
services.AddApplicationInsightsTelemetry(configuration["ApplicationInsights:InstrumentationKey"]);
services.AddLogging(builder =>
{
    builder.AddApplicationInsights();
    builder.AddConsole();
});
```

## CI/CD Pipeline (GitHub Actions)

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy E-Commerce App

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Build Docker images
      run: |
        docker build -t ecommerce/product-service:${{ github.sha }} ...
        docker build -t ecommerce/order-service:${{ github.sha }} ...
    
    - name: Push to ACR
      run: |
        docker push ecommerceacr.azurecr.io/ecommerce/product-service:${{ github.sha }}
        docker push ecommerceacr.azurecr.io/ecommerce/order-service:${{ github.sha }}
    
    - name: Deploy to AKS
      run: |
        az aks get-credentials ...
        kubectl set image deployment/product-service ...
        kubectl set image deployment/order-service ...
```

## Troubleshooting

### Service not communicating
- Check Service Bus connection string
- Verify firewall rules
- Check Docker network (if using Docker Compose)

### Database migration errors
- Ensure SQL Server is accessible
- Check connection strings
- Verify SQL Server authentication

### Event not being processed
- Check Service Bus emulator is running
- Verify subscription names match
- Check Application Insights logs

