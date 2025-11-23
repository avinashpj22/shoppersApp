import { createFeatureSelector, createSelector } from '@ngrx/store';
import { OrderState } from './order.reducer';

export const selectOrderState = createFeatureSelector<OrderState>('orders');

// Order List Selectors
export const selectAllOrders = createSelector(
  selectOrderState,
  (state: OrderState) => state.orders
);

export const selectOrdersLoading = createSelector(
  selectOrderState,
  (state: OrderState) => state.loading
);

export const selectOrdersError = createSelector(
  selectOrderState,
  (state: OrderState) => state.error
);

export const selectOrdersPagination = createSelector(
  selectOrderState,
  (state: OrderState) => ({
    pageNumber: state.pageNumber,
    pageSize: state.pageSize,
    totalCount: state.totalCount,
    totalPages: Math.ceil(state.totalCount / state.pageSize)
  })
);

// Single Order Selector
export const selectSelectedOrder = createSelector(
  selectOrderState,
  (state: OrderState) => state.selectedOrder
);

// Filtered Orders by Status
export const selectOrdersByStatus = (status: string) =>
  createSelector(
    selectAllOrders,
    (orders: any[]) =>
      status === 'All' ? orders : orders.filter(o => o.status === status)
  );

// Order Count
export const selectOrderCount = createSelector(
  selectAllOrders,
  (orders: any[]) => orders.length
);

// Total Spent
export const selectTotalSpent = createSelector(
  selectAllOrders,
  (orders: any[]) => orders.reduce((sum, order) => sum + order.totalAmount, 0)
);

// Orders with Pagination
export const selectOrdersWithPagination = createSelector(
  selectAllOrders,
  selectOrdersPagination,
  (orders, pagination) => ({
    orders,
    pagination
  })
);
