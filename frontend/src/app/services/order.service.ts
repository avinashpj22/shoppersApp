import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Order, OrderLineItem } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  constructor() { }

  getOrders(customerId: string, pageNumber: number = 1, pageSize: number = 10): Observable<{ orders: Order[]; totalCount: number }> {
    // Mock implementation - replace with HTTP call
    const mockOrders: Order[] = [
      {
        id: '1',
        customerId,
        lineItems: [],
        totalAmount: 150.00,
        status: 'Completed',
        createdAt: new Date().toISOString(),
        itemCount: 2
      }
    ];
    return of({ orders: mockOrders, totalCount: 1 });
  }

  getOrder(orderId: string): Observable<Order | null> {
    // Mock implementation
    return of(null);
  }

  placeOrder(customerId: string, lineItems: OrderLineItem[]): Observable<Order> {
    const order: Order = {
      id: Math.random().toString(36),
      customerId,
      lineItems,
      totalAmount: lineItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0),
      status: 'Pending',
      createdAt: new Date().toISOString(),
      itemCount: lineItems.length
    };
    return of(order);
  }

  confirmOrder(orderId: string): Observable<Order | null> {
    return of(null);
  }

  cancelOrder(orderId: string): Observable<Order | null> {
    return of(null);
  }
}
