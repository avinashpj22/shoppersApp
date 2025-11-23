import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { ProductState } from './store/product.reducer';
import * as ProductActions from './store/product.actions';
import { selectAllProducts, selectProductsLoading } from './store/product.selectors';

/**
 * Root Application Component
 * Manages main layout, navigation, and top-level state
 */
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'E-Commerce Platform';
  
  // Observable streams from NgRx store
  products$: Observable<any[]>;
  loading$: Observable<boolean>;
  
  // Navigation state
  navOpen = false;
  cartItemCount = 0;

  constructor(private store: Store<{ products: ProductState }>) {
    this.products$ = this.store.select(selectAllProducts);
    this.loading$ = this.store.select(selectProductsLoading);
  }

  ngOnInit(): void {
    // Load products on app initialization
    this.store.dispatch(ProductActions.loadProducts({
      pageNumber: 1,
      pageSize: 10
    }));

    // Subscribe to cart changes (from localStorage or service)
    this.loadCartCount();
  }

  /**
   * Load cart item count from localStorage
   */
  loadCartCount(): void {
    const cart = localStorage.getItem('cart');
    if (cart) {
      const items = JSON.parse(cart);
      this.cartItemCount = items.length;
    }
  }

  /**
   * Toggle navigation menu
   */
  toggleNav(): void {
    this.navOpen = !this.navOpen;
  }

  /**
   * Close navigation menu
   */
  closeNav(): void {
    this.navOpen = false;
  }

  /**
   * Handle search
   */
  onSearch(searchTerm: string): void {
    if (searchTerm.trim()) {
      this.store.dispatch(ProductActions.setProductFilter({
        category: undefined,
        minPrice: undefined,
        maxPrice: undefined,
        searchTerm: searchTerm
      }));
    }
  }

  /**
   * Handle category filter
   */
  onCategoryFilter(category: string): void {
    this.store.dispatch(ProductActions.setProductFilter({
      category: category,
      minPrice: undefined,
      maxPrice: undefined,
      searchTerm: undefined
    }));
  }
}
