import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ProductState } from './product.reducer';

/**
 * Product Selectors
 * Demonstrates NgRx selectors for efficient state queries.
 * Selectors are memoized and only recompute when selected slice changes.
 */

export const selectProductState = createFeatureSelector<ProductState>('products');

// ============================================================================
// Product List Selectors
// ============================================================================

export const selectAllProducts = createSelector(
  selectProductState,
  (state: ProductState) => state.items
);

export const selectProductsLoading = createSelector(
  selectProductState,
  (state: ProductState) => state.loading
);

export const selectProductsError = createSelector(
  selectProductState,
  (state: ProductState) => state.error
);

export const selectProductsPagination = createSelector(
  selectProductState,
  (state: ProductState) => ({
    pageNumber: state.pageNumber,
    pageSize: state.pageSize,
    totalCount: state.totalCount,
    totalPages: Math.ceil(state.totalCount / state.pageSize)
  })
);

export const selectProductsWithPagination = createSelector(
  selectAllProducts,
  selectProductsPagination,
  (products, pagination) => ({
    products,
    pagination
  })
);

// ============================================================================
// Product Detail Selectors
// ============================================================================

export const selectSelectedProduct = createSelector(
  selectProductState,
  (state: ProductState) => state.selectedProduct
);

export const selectProductById = (productId: string) =>
  createSelector(
    selectAllProducts,
    (products) => products.find(p => p.id === productId) || null
  );

// ============================================================================
// Filter Selectors
// ============================================================================

export const selectProductFilters = createSelector(
  selectProductState,
  (state: ProductState) => state.filters
);

export const selectProductsByCategory = (category: string) =>
  createSelector(
    selectAllProducts,
    (products) => products.filter(p => p.category === category)
  );

export const selectProductsByPriceRange = (minPrice: number, maxPrice: number) =>
  createSelector(
    selectAllProducts,
    (products) => products.filter(p => p.price >= minPrice && p.price <= maxPrice)
  );

// ============================================================================
// Inventory Check Selectors
// ============================================================================

export const selectInventoryCheck = createSelector(
  selectProductState,
  (state: ProductState) => state.inventoryCheck
);

export const selectIsInventoryAvailable = createSelector(
  selectInventoryCheck,
  (check) => check.isAvailable && check.checked
);

export const selectAvailableQuantity = createSelector(
  selectInventoryCheck,
  (check) => check.availableQuantity
);
