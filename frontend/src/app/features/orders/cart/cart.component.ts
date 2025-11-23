import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface CartItem {
  productId: string;
  name: string;
  price: number;
  quantity: number;
  category: string;
  sku?: string;
}

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit, OnDestroy {
  cartItems: CartItem[] = [];
  cartTotal = 0;
  subtotal = 0;
  tax = 0;
  shipping = 0;
  isEmpty = true;
  appliedDiscount = 0;
  discountCode = '';
  discountPercentage = 0;

  private TAX_RATE = 0.08;
  private SHIPPING_BASE = 10;
  private FREE_SHIPPING_THRESHOLD = 100;
  private destroy$ = new Subject<void>();

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.loadCart();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCart(): void {
    const cartData = localStorage.getItem('cart');
    this.cartItems = cartData ? JSON.parse(cartData) : [];
    this.isEmpty = this.cartItems.length === 0;
    this.calculateTotals();
  }

  calculateTotals(): void {
    // Calculate subtotal
    this.subtotal = this.cartItems.reduce((total, item) => {
      return total + (item.price * item.quantity);
    }, 0);

    // Calculate discount
    this.appliedDiscount = (this.subtotal * this.discountPercentage) / 100;

    // Calculate tax on subtotal minus discount
    const taxableAmount = this.subtotal - this.appliedDiscount;
    this.tax = taxableAmount * this.TAX_RATE;

    // Calculate shipping
    this.shipping = this.subtotal >= this.FREE_SHIPPING_THRESHOLD ? 0 : this.SHIPPING_BASE;

    // Calculate total
    this.cartTotal = taxableAmount + this.tax + this.shipping;
  }

  updateQuantity(productId: string, newQuantity: number): void {
    if (newQuantity > 0) {
      const item = this.cartItems.find(c => c.productId === productId);
      if (item) {
        item.quantity = newQuantity;
        this.saveCart();
        this.calculateTotals();
      }
    }
  }

  increaseQuantity(productId: string): void {
    const item = this.cartItems.find(c => c.productId === productId);
    if (item) {
      item.quantity++;
      this.saveCart();
      this.calculateTotals();
    }
  }

  decreaseQuantity(productId: string): void {
    const item = this.cartItems.find(c => c.productId === productId);
    if (item && item.quantity > 1) {
      item.quantity--;
      this.saveCart();
      this.calculateTotals();
    }
  }

  removeItem(productId: string): void {
    this.cartItems = this.cartItems.filter(item => item.productId !== productId);
    this.isEmpty = this.cartItems.length === 0;
    this.saveCart();
    this.calculateTotals();
  }

  clearCart(): void {
    if (confirm('Are you sure you want to clear your entire cart?')) {
      this.cartItems = [];
      this.isEmpty = true;
      this.discountCode = '';
      this.discountPercentage = 0;
      localStorage.removeItem('cart');
      this.calculateTotals();
    }
  }

  applyDiscount(): void {
    // Simple discount code validation
    // In a real app, this would call a backend API
    if (this.discountCode.toUpperCase() === 'SAVE10') {
      this.discountPercentage = 10;
      alert('Discount code applied! 10% off');
      this.calculateTotals();
    } else if (this.discountCode.toUpperCase() === 'SAVE20') {
      this.discountPercentage = 20;
      alert('Discount code applied! 20% off');
      this.calculateTotals();
    } else if (this.discountCode === '') {
      alert('Please enter a discount code');
    } else {
      alert('Invalid discount code');
      this.discountPercentage = 0;
      this.calculateTotals();
    }
  }

  removeDiscount(): void {
    this.discountCode = '';
    this.discountPercentage = 0;
    this.calculateTotals();
  }

  proceedToCheckout(): void {
    if (this.cartItems.length > 0) {
      // Store cart data for checkout
      localStorage.setItem('checkoutCart', JSON.stringify({
        items: this.cartItems,
        subtotal: this.subtotal,
        tax: this.tax,
        shipping: this.shipping,
        discount: this.appliedDiscount,
        total: this.cartTotal
      }));
      this.router.navigate(['/checkout']);
    } else {
      alert('Your cart is empty. Please add items before checkout.');
    }
  }

  continueShopping(): void {
    this.router.navigate(['/products']);
  }

  saveCart(): void {
    localStorage.setItem('cart', JSON.stringify(this.cartItems));
  }

  getItemTotal(item: CartItem): number {
    return item.price * item.quantity;
  }
}
