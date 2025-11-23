import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface OrderSummary {
  id: string;
  customerId: string;
  status: 'Pending' | 'Confirmed' | 'Shipped' | 'Completed' | 'Canceled' | 'Failed';
  totalAmount: number;
  itemCount: number;
  createdAt: Date;
  completedAt?: Date;
}

@Component({
  selector: 'app-order-history',
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.css']
})
export class OrderHistoryComponent implements OnInit, OnDestroy {
  orders: OrderSummary[] = [];
  filteredOrders: OrderSummary[] = [];
  loading = false;
  error: string | null = null;
  
  selectedStatus: string = 'All';
  statusOptions = ['All', 'Pending', 'Confirmed', 'Shipped', 'Completed', 'Canceled', 'Failed'];
  sortBy: 'date' | 'amount' = 'date';
  
  private destroy$ = new Subject<void>();

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = null;

    // Mock data - In production, this would call OrderService
    const mockOrders: OrderSummary[] = [
      {
        id: 'ORD-001',
        customerId: 'CUST-123',
        status: 'Completed',
        totalAmount: 159.99,
        itemCount: 3,
        createdAt: new Date('2025-11-15'),
        completedAt: new Date('2025-11-18')
      },
      {
        id: 'ORD-002',
        customerId: 'CUST-123',
        status: 'Shipped',
        totalAmount: 89.50,
        itemCount: 2,
        createdAt: new Date('2025-11-20'),
        completedAt: undefined
      },
      {
        id: 'ORD-003',
        customerId: 'CUST-123',
        status: 'Pending',
        totalAmount: 245.00,
        itemCount: 5,
        createdAt: new Date('2025-11-22'),
        completedAt: undefined
      },
      {
        id: 'ORD-004',
        customerId: 'CUST-123',
        status: 'Completed',
        totalAmount: 67.25,
        itemCount: 1,
        createdAt: new Date('2025-11-10'),
        completedAt: new Date('2025-11-12')
      },
      {
        id: 'ORD-005',
        customerId: 'CUST-123',
        status: 'Confirmed',
        totalAmount: 312.50,
        itemCount: 4,
        createdAt: new Date('2025-11-21'),
        completedAt: undefined
      }
    ];

    setTimeout(() => {
      this.orders = mockOrders;
      this.applyFilters();
      this.loading = false;
    }, 500);
  }

  applyFilters(): void {
    let filtered = this.orders;

    // Filter by status
    if (this.selectedStatus !== 'All') {
      filtered = filtered.filter(order => order.status === this.selectedStatus);
    }

    // Sort
    if (this.sortBy === 'date') {
      filtered.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    } else if (this.sortBy === 'amount') {
      filtered.sort((a, b) => b.totalAmount - a.totalAmount);
    }

    this.filteredOrders = filtered;
  }

  onStatusChange(status: string): void {
    this.selectedStatus = status;
    this.applyFilters();
  }

  onSortChange(sort: 'date' | 'amount'): void {
    this.sortBy = sort;
    this.applyFilters();
  }

  viewOrderDetails(orderId: string): void {
    this.router.navigate(['/orders', orderId]);
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Completed':
        return 'status-completed';
      case 'Shipped':
        return 'status-shipped';
      case 'Confirmed':
        return 'status-confirmed';
      case 'Pending':
        return 'status-pending';
      case 'Canceled':
        return 'status-canceled';
      case 'Failed':
        return 'status-failed';
      default:
        return '';
    }
  }

  getStatusBadge(status: string): string {
    switch (status) {
      case 'Completed':
        return '✓ Completed';
      case 'Shipped':
        return '📦 Shipped';
      case 'Confirmed':
        return '✓ Confirmed';
      case 'Pending':
        return '⏳ Pending';
      case 'Canceled':
        return '✕ Canceled';
      case 'Failed':
        return '✕ Failed';
      default:
        return status;
    }
  }

  retryLoad(): void {
    this.loadOrders();
  }
}
