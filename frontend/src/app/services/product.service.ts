import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, PagedProductResult, InventoryCheck, Order, CreateOrderRequest } from '../models/product.model';

/**
 * Product Service
 * Handles all API communication for product-related operations.
 * Communicates with Product Service microservice via API Gateway.
 */
@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly apiUrl = 'http://localhost:8000/api/v1'; // API Gateway URL

  constructor(private http: HttpClient) {}

  /**
   * Get all products with optional filtering and pagination.
   */
  getProducts(
    pageNumber: number = 1,
    pageSize: number = 10,
    category?: string,
    minPrice?: number,
    maxPrice?: number,
    searchTerm?: string
  ): Observable<PagedProductResult> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (category) params = params.set('category', category);
    if (minPrice !== undefined) params = params.set('minPrice', minPrice.toString());
    if (maxPrice !== undefined) params = params.set('maxPrice', maxPrice.toString());
    if (searchTerm) params = params.set('searchTerm', searchTerm);

    return this.http.get<PagedProductResult>(
      `${this.apiUrl}/products`,
      { params }
    );
  }

  /**
   * Get a single product by ID.
   */
  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/products/${id}`);
  }

  /**
   * Get products by category.
   */
  getProductsByCategory(category: string): Observable<Product[]> {
    return this.http.get<Product[]>(
      `${this.apiUrl}/products/category/${category}`
    );
  }

  /**
   * Check inventory availability.
   */
  checkInventory(productId: string, quantity: number): Observable<InventoryCheck> {
    let params = new HttpParams()
      .set('productId', productId)
      .set('quantity', quantity.toString());

    return this.http.post<InventoryCheck>(
      `${this.apiUrl}/products/inventory/check`,
      null,
      { params }
    );
  }
}

/**
 * Order Service
 * Handles all API communication for order-related operations.
 * Communicates with Order Service microservice via API Gateway.
 */
@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiUrl = 'http://localhost:8000/api/v1'; // API Gateway URL

  constructor(private http: HttpClient) {}

  /**
   * Get an order by ID.
   */
  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/orders/${id}`);
  }

  /**
   * Get all orders for a customer.
   */
  getCustomerOrders(
    customerId: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<any> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(
      `${this.apiUrl}/orders/customer/${customerId}`,
      { params }
    );
  }

  /**
   * Get orders by status (admin/dashboard).
   */
  getOrdersByStatus(
    status: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<any> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(
      `${this.apiUrl}/orders/status/${status}`,
      { params }
    );
  }

  /**
   * Get order statistics (dashboard).
   */
  getOrderStatistics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/orders/statistics`);
  }

  /**
   * Place a new order.
   */
  placeOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(
      `${this.apiUrl}/orders`,
      request
    );
  }

  /**
   * Confirm an order (after payment).
   */
  confirmOrder(orderId: string): Observable<Order> {
    return this.http.put<Order>(
      `${this.apiUrl}/orders/${orderId}/confirm`,
      null
    );
  }

  /**
   * Ship an order.
   */
  shipOrder(orderId: string, trackingNumber: string): Observable<Order> {
    return this.http.put<Order>(
      `${this.apiUrl}/orders/${orderId}/ship`,
      { trackingNumber }
    );
  }

  /**
   * Cancel an order.
   */
  cancelOrder(orderId: string): Observable<Order> {
    return this.http.put<Order>(
      `${this.apiUrl}/orders/${orderId}/cancel`,
      null
    );
  }
}
