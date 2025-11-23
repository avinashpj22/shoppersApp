import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import * as OrderActions from './order.actions';
import { OrderService } from '../services/order.service';

@Injectable()
export class OrderEffects {
  loadOrders$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadOrders),
      switchMap((action: any) =>
        this.orderService.getOrdersByStatus(action.customerId).pipe(
          map((response: any) =>
            OrderActions.loadOrdersSuccess({
              orders: response.orders || [],
              totalCount: response.totalCount || 0
            })
          ),
          catchError((error: any) =>
            of(OrderActions.loadOrdersFailure({ error: error.message }))
          )
        )
      )
    )
  );

  getOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.getOrder),
      switchMap((action: any) =>
        this.orderService.getOrder(action.orderId).pipe(
          map((order: any) => OrderActions.getOrderSuccess({ order })),
          catchError((error: any) =>
            of(OrderActions.getOrderFailure({ error: error.message }))
          )
        )
      )
    )
  );

  placeOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.placeOrder),
      switchMap((action: any) =>
        this.orderService.placeOrder({
          customerId: action.customerId,
          lineItems: action.lineItems
        }).pipe(
          map((order: any) => OrderActions.placeOrderSuccess({ order })),
          catchError((error: any) =>
            of(OrderActions.placeOrderFailure({ error: error.message }))
          )
        )
      )
    )
  );

  confirmOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.confirmOrder),
      switchMap((action: any) =>
        this.orderService.confirmOrder(action.orderId).pipe(
          map((order: any) => OrderActions.confirmOrderSuccess({ order })),
          catchError((error: any) =>
            of(OrderActions.loadOrdersFailure({ error: error.message }))
          )
        )
      )
    )
  );

  cancelOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.cancelOrder),
      switchMap((action: any) =>
        this.orderService.cancelOrder(action.orderId).pipe(
          map((order: any) => OrderActions.cancelOrderSuccess({ order })),
          catchError((error: any) =>
            of(OrderActions.loadOrdersFailure({ error: error.message }))
          )
        )
      )
    )
  );

  constructor(private actions$: Actions, private orderService: OrderService) {}
}
