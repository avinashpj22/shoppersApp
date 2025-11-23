import { createReducer, on } from '@ngrx/store';
import * as OrderActions from './order.actions';
import { Order } from '../models/product.model';

export interface OrderState {
  orders: Order[];
  selectedOrder: Order | null;
  loading: boolean;
  error: string | null;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export const initialOrderState: OrderState = {
  orders: [],
  selectedOrder: null,
  loading: false,
  error: null,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0
};

export const orderReducer = createReducer(
  initialOrderState,

  // Load Orders
  on(OrderActions.loadOrders, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(OrderActions.loadOrdersSuccess, (state, { orders, totalCount }) => ({
    ...state,
    orders,
    totalCount,
    loading: false,
    error: null
  })),

  on(OrderActions.loadOrdersFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Get Single Order
  on(OrderActions.getOrder, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(OrderActions.getOrderSuccess, (state, { order }) => ({
    ...state,
    selectedOrder: order,
    loading: false,
    error: null
  })),

  on(OrderActions.getOrderFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Place Order
  on(OrderActions.placeOrder, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(OrderActions.placeOrderSuccess, (state, { order }) => ({
    ...state,
    selectedOrder: order,
    orders: [order, ...state.orders],
    loading: false,
    error: null
  })),

  on(OrderActions.placeOrderFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Confirm Order
  on(OrderActions.confirmOrder, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(OrderActions.confirmOrderSuccess, (state, { order }) => ({
    ...state,
    selectedOrder: order,
    orders: state.orders.map(o => o.id === order.id ? order : o),
    loading: false,
    error: null
  })),

  // Cancel Order
  on(OrderActions.cancelOrder, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(OrderActions.cancelOrderSuccess, (state, { order }) => ({
    ...state,
    selectedOrder: order,
    orders: state.orders.map(o => o.id === order.id ? order : o),
    loading: false,
    error: null
  })),

  // Clear State
  on(OrderActions.clearOrderState, (state) => ({
    ...initialOrderState
  }))
);
