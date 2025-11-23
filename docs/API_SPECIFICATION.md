# API Specification & Usage Guide

## Base URL

All endpoints are accessed through the API Gateway:
- **Development**: `http://localhost:8000`
- **Production**: `https://api.ecommerce.example.com`

## Authentication

All endpoints require JWT Bearer token in the Authorization header:
```
Authorization: Bearer <your_jwt_token>
```

## Common Response Codes

- `200 OK` - Successful GET/PUT request
- `201 Created` - Successful POST request
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Invalid input data
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Request conflicts with current state
- `500 Internal Server Error` - Server error

---

## Product Service API

### Get All Products (Paginated)

```
GET /api/v1/products
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| pageNumber | integer | false | Page number (default: 1) |
| pageSize | integer | false | Items per page (default: 10) |
| category | string | false | Filter by category |
| minPrice | decimal | false | Minimum price filter |
| maxPrice | decimal | false | Maximum price filter |
| searchTerm | string | false | Search in name/description |

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Laptop Pro",
      "description": "High-performance laptop",
      "price": 1299.99,
      "stockQuantity": 15,
      "sku": "LAPTOP-PRO-001",
      "category": "Electronics",
      "isActive": true
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Example:**
```bash
curl -X GET "http://localhost:8000/api/v1/products?pageNumber=1&pageSize=10&category=Electronics" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### Get Product by ID

```
GET /api/v1/products/{id}
```

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | guid | Product ID |

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Laptop Pro",
  "description": "High-performance laptop",
  "price": 1299.99,
  "stockQuantity": 15,
  "sku": "LAPTOP-PRO-001",
  "category": "Electronics",
  "isActive": true
}
```

**Example:**
```bash
curl -X GET "http://localhost:8000/api/v1/products/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### Create Product

```
POST /api/v1/products
```

**Request Body:**
```json
{
  "name": "Laptop Pro",
  "description": "High-performance laptop",
  "price": 1299.99,
  "stockQuantity": 15,
  "sku": "LAPTOP-PRO-001",
  "category": "Electronics"
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Laptop Pro",
  "description": "High-performance laptop",
  "price": 1299.99,
  "stockQuantity": 15,
  "sku": "LAPTOP-PRO-001",
  "category": "Electronics",
  "isActive": true
}
```

**Example:**
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

---

### Update Product

```
PUT /api/v1/products/{id}
```

**Request Body:**
```json
{
  "name": "Laptop Pro Plus",
  "description": "Updated description",
  "price": 1399.99,
  "category": "Electronics"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Laptop Pro Plus",
  "description": "Updated description",
  "price": 1399.99,
  "stockQuantity": 15,
  "sku": "LAPTOP-PRO-001",
  "category": "Electronics",
  "isActive": true
}
```

---

### Check Inventory

```
POST /api/v1/products/inventory/check
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| productId | guid | true | Product ID |
| quantity | integer | true | Quantity to check |

**Response (200 OK):**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "requestedQuantity": 5,
  "availableQuantity": 15,
  "isAvailable": true
}
```

**Example:**
```bash
curl -X POST "http://localhost:8000/api/v1/products/inventory/check?productId=550e8400-e29b-41d4-a716-446655440000&quantity=5" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Order Service API

### Get All Orders (Paginated)

```
GET /api/v1/orders
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | integer | Page number (default: 1) |
| pageSize | integer | Items per page (default: 10) |

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "customerId": "770e8400-e29b-41d4-a716-446655440000",
      "status": "Confirmed",
      "totalAmount": 1299.99,
      "createdAt": "2024-01-15T10:30:00Z",
      "completedAt": null,
      "lineItems": [
        {
          "productId": "550e8400-e29b-41d4-a716-446655440000",
          "productName": "Laptop Pro",
          "quantity": 1,
          "unitPrice": 1299.99
        }
      ]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5
}
```

---

### Get Order by ID

```
GET /api/v1/orders/{id}
```

**Response (200 OK):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "customerId": "770e8400-e29b-41d4-a716-446655440000",
  "status": "Confirmed",
  "totalAmount": 1299.99,
  "createdAt": "2024-01-15T10:30:00Z",
  "completedAt": null,
  "lineItems": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "productName": "Laptop Pro",
      "quantity": 1,
      "unitPrice": 1299.99
    }
  ]
}
```

---

### Place Order

```
POST /api/v1/orders
```

**Request Body:**
```json
{
  "customerId": "770e8400-e29b-41d4-a716-446655440000",
  "lineItems": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "productName": "Laptop Pro",
      "quantity": 1,
      "unitPrice": 1299.99
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "customerId": "770e8400-e29b-41d4-a716-446655440000",
  "status": "Pending",
  "totalAmount": 1299.99,
  "createdAt": "2024-01-15T10:30:00Z",
  "completedAt": null,
  "lineItems": [...]
}
```

---

### Confirm Order

```
PUT /api/v1/orders/{id}/confirm
```

**Response (200 OK):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "customerId": "770e8400-e29b-41d4-a716-446655440000",
  "status": "Confirmed",
  "totalAmount": 1299.99,
  "createdAt": "2024-01-15T10:30:00Z",
  "completedAt": null,
  "lineItems": [...]
}
```

---

### Ship Order

```
PUT /api/v1/orders/{id}/ship
```

**Request Body:**
```json
{
  "trackingNumber": "TRACK123456789"
}
```

**Response (200 OK):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "status": "Shipped",
  "totalAmount": 1299.99,
  "createdAt": "2024-01-15T10:30:00Z",
  "completedAt": "2024-01-15T14:45:00Z",
  "lineItems": [...]
}
```

---

### Cancel Order

```
PUT /api/v1/orders/{id}/cancel
```

**Response (200 OK):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "status": "Canceled",
  "totalAmount": 1299.99,
  "createdAt": "2024-01-15T10:30:00Z",
  "completedAt": null,
  "lineItems": [...]
}
```

---

### Get Order Statistics

```
GET /api/v1/orders/statistics
```

**Response (200 OK):**
```json
{
  "totalOrders": 1250,
  "totalRevenue": 1524875.50,
  "completedOrders": 1000,
  "pendingOrders": 150,
  "canceledOrders": 100,
  "averageOrderValue": 1219.90
}
```

---

## Error Handling

All errors follow this format:

```json
{
  "message": "Error description",
  "code": "ERROR_CODE",
  "details": [
    "Additional detail 1",
    "Additional detail 2"
  ]
}
```

**Example Error Response (400 Bad Request):**
```json
{
  "message": "Order must have at least one line item",
  "code": "INVALID_ORDER",
  "details": []
}
```

---

## Rate Limiting

- **Limit**: 1000 requests per minute per API key
- **Header**: `X-RateLimit-Remaining`

---

## SDK Examples

### JavaScript/TypeScript (Angular)

```typescript
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable()
export class ApiClient {
  constructor(private http: HttpClient) {}

  getProducts(page: number = 1) {
    return this.http.get('/api/v1/products', {
      params: { pageNumber: page, pageSize: 10 }
    });
  }

  placeOrder(order: any) {
    return this.http.post('/api/v1/orders', order);
  }
}
```

### C# (.NET)

```csharp
using System.Net.Http;
using System.Threading.Tasks;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Product> GetProductAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/products/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<Product>();
    }
}
```

---

## Webhook Events

Orders Service publishes events to Azure Service Bus:

- **OrderCreated**: Order has been placed
- **OrderConfirmed**: Payment processed
- **OrderShipped**: Order has shipped
- **OrderCompleted**: Order delivered
- **OrderCanceled**: Order has been canceled

