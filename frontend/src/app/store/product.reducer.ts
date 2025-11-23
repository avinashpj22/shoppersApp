import { createReducer, on } from '@ngrx/store';
import * as ProductActions from './product.actions';
import { Product } from '../models/product.model';

/**
 * Product State Interface
 * Defines the shape of the product state in NgRx store.
 */
export interface ProductState {
  items: Product[];
  selectedProduct: Product | null;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  loading: boolean;
  error: string | null;
  filters: {
    category?: string;
    minPrice?: number;
    maxPrice?: number;
    searchTerm?: string;
  };
  inventoryCheck: {
    productId: string | null;
    availableQuantity: number;
    isAvailable: boolean;
    checked: boolean;
  };
}

/**
 * Initial state
 */
export const initialProductState: ProductState = {
  items: [],
  selectedProduct: null,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0,
  loading: false,
  error: null,
  filters: {},
  inventoryCheck: {
    productId: null,
    availableQuantity: 0,
    isAvailable: false,
    checked: false
  }
};

/**
 * Product Reducer
 * Pure function that updates state based on actions.
 * Demonstrates immutable state updates and CQRS pattern.
 */
export const productReducer = createReducer(
  initialProductState,

  // Load Products
  on(ProductActions.loadProducts, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(ProductActions.loadProductsSuccess, (state, action) => ({
    ...state,
    items: action.products,
    pageNumber: action.pageNumber,
    pageSize: action.pageSize,
    totalCount: action.totalCount,
    loading: false,
    error: null
  })),

  on(ProductActions.loadProductsFailure, (state, action) => ({
    ...state,
    loading: false,
    error: action.error
  })),

  // Load Single Product
  on(ProductActions.loadProduct, (state) => ({
    ...state,
    loading: true,
    error: null
  })),

  on(ProductActions.loadProductSuccess, (state, action) => ({
    ...state,
    selectedProduct: action.product,
    loading: false,
    error: null
  })),

  on(ProductActions.loadProductFailure, (state, action) => ({
    ...state,
    loading: false,
    error: action.error
  })),

  // Filter Actions
  on(ProductActions.setProductFilter, (state, action) => ({
    ...state,
    filters: {
      ...state.filters,
      ...action
    },
    pageNumber: 1 // Reset to first page when filter changes
  })),

  on(ProductActions.clearProductFilters, (state) => ({
    ...state,
    filters: {},
    pageNumber: 1
  })),

  // Inventory Check
  on(ProductActions.checkInventory, (state) => ({
    ...state,
    inventoryCheck: {
      ...state.inventoryCheck,
      checked: false
    }
  })),

  on(ProductActions.checkInventorySuccess, (state, action) => ({
    ...state,
    inventoryCheck: {
      productId: action.productId,
      availableQuantity: action.availableQuantity,
      isAvailable: action.isAvailable,
      checked: true
    }
  })),

  on(ProductActions.checkInventoryFailure, (state) => ({
    ...state,
    inventoryCheck: {
      ...state.inventoryCheck,
      checked: false
    }
  }))
);
