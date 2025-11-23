import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import * as ProductActions from '../../../store/product.actions';
import {
  selectAllProducts,
  selectProductsLoading,
  selectProductsError,
  selectProductsPagination
} from '../../../store/product.selectors';
import { Product, PagedProductResult } from '../../../models/product.model';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {
  products$: Observable<Product[]>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  pagination$: Observable<{ pageNumber: number; pageSize: number; totalCount: number; totalPages: number }>;

  currentPage = 1;
  pageSize = 12;
  sortBy: 'name' | 'price' | 'newest' = 'name';
  viewMode: 'grid' | 'list' = 'grid';

  constructor(
    private store: Store<{ products: any }>,
    private router: Router
  ) {
    this.products$ = this.store.select(selectAllProducts);
    this.loading$ = this.store.select(selectProductsLoading);
    this.error$ = this.store.select(selectProductsError);
    this.pagination$ = this.store.select(selectProductsPagination);
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.store.dispatch(ProductActions.loadProducts({
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }));
  }

  onSortChange(sort: string): void {
    this.sortBy = sort as 'name' | 'price' | 'newest';
    this.currentPage = 1;
    // Sort logic could be implemented in reducer or as a separate effect
    this.loadProducts();
  }

  onViewModeChange(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  onPageChange(pageNumber: number): void {
    this.currentPage = pageNumber;
    this.loadProducts();
  }

  onPageSizeChange(pageSize: number): void {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadProducts();
  }

  viewProductDetail(productId: string): void {
    this.router.navigate(['/products', productId]);
  }

  addToCart(product: Product): void {
    // Dispatch add to cart action - to be implemented with CartService
    // For now, we'll use localStorage
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    const existingItem = cart.find((item: any) => item.productId === product.id);
    
    if (existingItem) {
      existingItem.quantity += 1;
    } else {
      cart.push({
        productId: product.id,
        name: product.name,
        price: product.price,
        quantity: 1,
        category: product.category
      });
    }
    
    localStorage.setItem('cart', JSON.stringify(cart));
    alert('Product added to cart!');
  }

  onCategoryChange(category: string): void {
    this.currentPage = 1;
    this.store.dispatch(ProductActions.setProductFilter({
      category: category,
      minPrice: undefined,
      maxPrice: undefined
    }));
    this.loadProducts();
  }

  onPriceRangeChange(minPrice: number, maxPrice: number): void {
    this.currentPage = 1;
    this.store.dispatch(ProductActions.setProductFilter({
      category: undefined,
      minPrice: minPrice,
      maxPrice: maxPrice
    }));
    this.loadProducts();
  }

  clearFilters(): void {
    this.currentPage = 1;
    this.store.dispatch(ProductActions.setProductFilter({
      category: undefined,
      minPrice: undefined,
      maxPrice: undefined
    }));
    this.loadProducts();
  }

  retryLoadProducts(): void {
    this.loadProducts();
  }
}
