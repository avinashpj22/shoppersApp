import { createAction, props } from '@ngrx/store';
import { Order, OrderLineItem } from '../models/product.model';

// Load Orders
export const loadOrders = createAction(
  '[Order Page] Load Orders',
  props<{ customerId: string; pageNumber?: number; pageSize?: number }>()
);

export const loadOrdersSuccess = createAction(
  '[Order API] Load Orders Success',
  props<{ orders: Order[]; totalCount: number }>()
);

export const loadOrdersFailure = createAction(
  '[Order API] Load Orders Failure',
  props<{ error: string }>()
);

// Get Single Order
export const getOrder = createAction(
  '[Order Details Page] Get Order',
  props<{ orderId: string }>()
);

export const getOrderSuccess = createAction(
  '[Order API] Get Order Success',
  props<{ order: Order }>()
);

export const getOrderFailure = createAction(
  '[Order API] Get Order Failure',
  props<{ error: string }>()
);

// Place Order
export const placeOrder = createAction(
  '[Checkout Page] Place Order',
  props<{ customerId: string; lineItems: OrderLineItem[] }>()
);

export const placeOrderSuccess = createAction(
  '[Order API] Place Order Success',
  props<{ order: Order }>()
);

export const placeOrderFailure = createAction(
  '[Order API] Place Order Failure',
  props<{ error: string }>()
);

// Confirm Order
export const confirmOrder = createAction(
  '[Order] Confirm Order',
  props<{ orderId: string }>()
);

export const confirmOrderSuccess = createAction(
  '[Order API] Confirm Order Success',
  props<{ order: Order }>()
);

// Cancel Order
export const cancelOrder = createAction(
  '[Order] Cancel Order',
  props<{ orderId: string }>()
);

export const cancelOrderSuccess = createAction(
  '[Order API] Cancel Order Success',
  props<{ order: Order }>()
);

// Clear Order State
export const clearOrderState = createAction(
  '[Order] Clear State'
);
