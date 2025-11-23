import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as ProductActions from '../../../store/product.actions';
import {
  selectAllProducts,
  selectProductsLoading,
  selectProductsError,
  selectInventoryCheck
} from '../../../store/product.selectors';
import { Product, InventoryCheck } from '../../../models/product.model';

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  product: Product | null = null;
  products$: Observable<Product[]>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  inventoryCheck$: Observable<InventoryCheck | null>;

  quantity = 1;
  selectedProduct: Product | null = null;
  inventoryCheckResult: InventoryCheck | null = null;
  showCheckInventory = false;

  private productId: string = '';
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private store: Store<{ products: any }>
  ) {
    this.products$ = this.store.select(selectAllProducts);
    this.loading$ = this.store.select(selectProductsLoading);
    this.error$ = this.store.select(selectProductsError);
    this.inventoryCheck$ = this.store.select(selectInventoryCheck);
  }

  ngOnInit(): void {
    // Get product ID from route params
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe((params: any) => {
        this.productId = params['id'];
        this.loadProductDetail();
      });

    // Subscribe to inventory check result
    this.inventoryCheck$
      .pipe(takeUntil(this.destroy$))
      .subscribe((result: any) => {
        this.inventoryCheckResult = result;
      });

    // Subscribe to products list and find the detail
    this.products$
      .pipe(takeUntil(this.destroy$))
      .subscribe((products: Product[]) => {
        this.selectedProduct = products.find((p: Product) => p.id === this.productId) || null;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProductDetail(): void {
    // Load the specific product by ID
    this.store.dispatch(ProductActions.loadProduct({
      id: this.productId
    }));
  }

  updateQuantity(newQuantity: number): void {
    if (newQuantity > 0 && newQuantity <= (this.selectedProduct?.stockQuantity || 0)) {
      this.quantity = newQuantity;
    }
  }

  decreaseQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  increaseQuantity(): void {
    if (this.selectedProduct && this.quantity < this.selectedProduct.stockQuantity) {
      this.quantity++;
    }
  }

  checkInventory(): void {
    if (this.selectedProduct) {
      this.showCheckInventory = true;
      this.store.dispatch(ProductActions.checkInventory({
        productId: this.selectedProduct.id,
        quantity: this.quantity
      }));
    }
  }

  addToCart(): void {
    if (this.selectedProduct) {
      const cart = JSON.parse(localStorage.getItem('cart') || '[]');
      const existingItem = cart.find((item: any) => item.productId === this.selectedProduct!.id);

      if (existingItem) {
        existingItem.quantity += this.quantity;
      } else {
        cart.push({
          productId: this.selectedProduct.id,
          name: this.selectedProduct.name,
          price: this.selectedProduct.price,
          quantity: this.quantity,
          category: this.selectedProduct.category,
          sku: this.selectedProduct.sku
        });
      }

      localStorage.setItem('cart', JSON.stringify(cart));
      alert(`Added ${this.quantity} item(s) to cart!`);
      this.quantity = 1;
    }
  }

  proceedToCheckout(): void {
    if (this.selectedProduct) {
      this.addToCart();
      this.router.navigate(['/checkout']);
    }
  }

  backToProducts(): void {
    this.router.navigate(['/products']);
  }
}
