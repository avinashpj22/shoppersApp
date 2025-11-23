import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from '../../../services/cart.service';
import { CartItem } from '../../../models/product.model';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cartItems: CartItem[] = [];
  discountCode: string = '';
  discountApplied: boolean = false;

  constructor(private cartService: CartService, private router: Router) { }

  ngOnInit(): void {
    this.cartService.getCartItems().subscribe(items => {
      this.cartItems = items;
    });
  }

  removeItem(productId: string): void {
    this.cartService.removeFromCart(productId);
  }

  updateQuantity(productId: string, quantity: number): void {
    this.cartService.updateQuantity(productId, quantity);
  }

  clearCart(): void {
    this.cartService.clearCart();
  }

  applyDiscount(): void {
    if (this.discountCode === 'SAVE10' || this.discountCode === 'SAVE20') {
      this.discountApplied = true;
    }
  }

  getSubtotal(): number {
    return this.cartItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
  }

  getDiscount(): number {
    if (!this.discountApplied) return 0;
    const subtotal = this.getSubtotal();
    if (this.discountCode === 'SAVE10') return subtotal * 0.10;
    if (this.discountCode === 'SAVE20') return subtotal * 0.20;
    return 0;
  }

  getTax(): number {
    return (this.getSubtotal() - this.getDiscount()) * 0.08;
  }

  getShipping(): number {
    const subtotal = this.getSubtotal();
    return subtotal > 100 ? 0 : 9.99;
  }

  getTotal(): number {
    return this.getSubtotal() - this.getDiscount() + this.getTax() + this.getShipping();
  }

  checkout(): void {
    this.router.navigate(['/checkout']);
  }
}
