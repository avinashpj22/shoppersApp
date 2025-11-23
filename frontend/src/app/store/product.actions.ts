import { createAction, props } from '@ngrx/store';
import { Product } from '../models/product.model';

/**
 * Product Actions
 * Demonstrates NgRx action pattern for state management.
 * Actions are dispatched from components and processed by reducers and effects.
 */

// ============================================================================
// Product List Actions
// ============================================================================

export const loadProducts = createAction(
  '[Product Page] Load Products',
  props<{
    pageNumber: number;
    pageSize: number;
    category?: string;
    minPrice?: number;
    maxPrice?: number;
  }>()
);

export const loadProductsSuccess = createAction(
  '[Product API] Load Products Success',
  props<{
    products: Product[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
  }>()
);

export const loadProductsFailure = createAction(
  '[Product API] Load Products Failure',
  props<{ error: string }>()
);

// ============================================================================
// Product Detail Actions
// ============================================================================

export const loadProduct = createAction(
  '[Product Details Page] Load Product',
  props<{ id: string }>()
);

export const loadProductSuccess = createAction(
  '[Product API] Load Product Success',
  props<{ product: Product }>()
);

export const loadProductFailure = createAction(
  '[Product API] Load Product Failure',
  props<{ error: string }>()
);

// ============================================================================
// Product Search/Filter Actions
// ============================================================================

export const setProductFilter = createAction(
  '[Product Filter] Set Filter',
  props<{
    category?: string;
    minPrice?: number;
    maxPrice?: number;
    searchTerm?: string;
  }>()
);

export const clearProductFilters = createAction(
  '[Product Filter] Clear Filters'
);

// ============================================================================
// Inventory Check Actions
// ============================================================================

export const checkInventory = createAction(
  '[Product Page] Check Inventory',
  props<{ productId: string; quantity: number }>()
);

export const checkInventorySuccess = createAction(
  '[Product API] Check Inventory Success',
  props<{
    productId: string;
    availableQuantity: number;
    isAvailable: boolean;
  }>()
);

export const checkInventoryFailure = createAction(
  '[Product API] Check Inventory Failure',
  props<{ error: string }>()
);
