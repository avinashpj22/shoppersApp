import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import * as ProductActions from './product.actions';
import { ProductService } from '../services/product.service';

/**
 * Product Effects
 * Demonstrates NgRx effects for handling side effects (API calls) in CQRS pattern.
 * Effects are triggered by actions and call services to perform operations.
 */
@Injectable()
export class ProductEffects {
  loadProducts$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProductActions.loadProducts),
      switchMap(action =>
        this.productService.getProducts(
          action.pageNumber,
          action.pageSize,
          action.category,
          action.minPrice,
          action.maxPrice
        ).pipe(
          map(result => ProductActions.loadProductsSuccess({
            products: result.items,
            pageNumber: result.pageNumber,
            pageSize: result.pageSize,
            totalCount: result.totalCount
          })),
          catchError(error => of(ProductActions.loadProductsFailure({ error: error.message })))
        )
      )
    )
  );

  loadProduct$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProductActions.loadProduct),
      switchMap(action =>
        this.productService.getProduct(action.id).pipe(
          map(product => ProductActions.loadProductSuccess({ product })),
          catchError(error => of(ProductActions.loadProductFailure({ error: error.message })))
        )
      )
    )
  );

  checkInventory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProductActions.checkInventory),
      switchMap(action =>
        this.productService.checkInventory(action.productId, action.quantity).pipe(
          map(result => ProductActions.checkInventorySuccess({
            productId: result.productId,
            availableQuantity: result.availableQuantity,
            isAvailable: result.isAvailable
          })),
          catchError(error => of(ProductActions.checkInventoryFailure({ error: error.message })))
        )
      )
    )
  );

  constructor(
    private actions$: Actions,
    private productService: ProductService
  ) {}
}
