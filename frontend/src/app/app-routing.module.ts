import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ProductListComponent } from './features/products/product-list/product-list.component';
import { ProductDetailComponent } from './features/products/product-detail/product-detail.component';
import { CartComponent } from './features/products/cart/cart.component';
import { CheckoutComponent } from './features/orders/checkout/checkout.component';
import { OrderHistoryComponent } from './features/orders/order-history/order-history.component';

/**
 * Application Routing Configuration
 * Defines all routes for the e-commerce application
 */
const routes: Routes = [
  {
    path: '',
    redirectTo: '/products',
    pathMatch: 'full'
  },
  {
    path: 'products',
    component: ProductListComponent,
    data: { title: 'Products' }
  },
  {
    path: 'products/:id',
    component: ProductDetailComponent,
    data: { title: 'Product Details' }
  },
  {
    path: 'cart',
    component: CartComponent,
    data: { title: 'Shopping Cart' }
  },
  {
    path: 'checkout',
    component: CheckoutComponent,
    data: { title: 'Checkout' }
  },
  {
    path: 'orders',
    component: OrderHistoryComponent,
    data: { title: 'Order History' }
  },
  {
    path: '**',
    redirectTo: '/products'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
