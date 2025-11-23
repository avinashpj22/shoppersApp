import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { CartItem } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItems$ = new BehaviorSubject<CartItem[]>([]);

  constructor() {
    this.loadCartFromStorage();
  }

  getCartItems(): Observable<CartItem[]> {
    return this.cartItems$.asObservable();
  }

  addToCart(item: CartItem): void {
    const currentCart = this.cartItems$.value;
    const existingItem = currentCart.find(i => i.productId === item.productId);
    
    if (existingItem) {
      existingItem.quantity += item.quantity;
    } else {
      currentCart.push(item);
    }
    
    this.cartItems$.next([...currentCart]);
    this.saveCartToStorage();
  }

  removeFromCart(productId: string): void {
    const updatedCart = this.cartItems$.value.filter(item => item.productId !== productId);
    this.cartItems$.next(updatedCart);
    this.saveCartToStorage();
  }

  clearCart(): void {
    this.cartItems$.next([]);
    this.saveCartToStorage();
  }

  updateQuantity(productId: string, quantity: number): void {
    const currentCart = this.cartItems$.value;
    const item = currentCart.find(i => i.productId === productId);
    
    if (item) {
      if (quantity <= 0) {
        this.removeFromCart(productId);
      } else {
        item.quantity = quantity;
        this.cartItems$.next([...currentCart]);
        this.saveCartToStorage();
      }
    }
  }

  private saveCartToStorage(): void {
    localStorage.setItem('cart', JSON.stringify(this.cartItems$.value));
  }

  private loadCartFromStorage(): void {
    const savedCart = localStorage.getItem('cart');
    if (savedCart) {
      try {
        this.cartItems$.next(JSON.parse(savedCart));
      } catch {
        this.cartItems$.next([]);
      }
    }
  }
}
