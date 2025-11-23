import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as OrderActions from '../../../store/order.actions';
import { CreateOrderRequest, OrderLineItem } from '../../../models/product.model';
import { OrderService } from '../../../services/order.service';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit, OnDestroy {
  shippingForm!: FormGroup;
  billingForm!: FormGroup;
  paymentForm!: FormGroup;
  
  cartData: any = null;
  orderItems: OrderLineItem[] = [];
  
  currentStep = 1;
  isProcessing = false;
  orderPlaced = false;
  orderId: string | null = null;
  
  sameAsBilling = true;
  orderSubmitted = false;

  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private store: Store<{ orders: any }>,
    private orderService: OrderService
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadCartData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForms(): void {
    // Shipping Form
    this.shippingForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      addressLine1: ['', [Validators.required]],
      addressLine2: [''],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}$/)]],
      country: ['', [Validators.required]]
    });

    // Billing Form
    this.billingForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      addressLine1: ['', [Validators.required]],
      addressLine2: [''],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}$/)]],
      country: ['', [Validators.required]]
    });

    // Payment Form
    this.paymentForm = this.formBuilder.group({
      cardNumber: ['', [Validators.required, Validators.pattern(/^\d{16}$/)]],
      cardholderName: ['', [Validators.required, Validators.minLength(5)]],
      expiryDate: ['', [Validators.required, Validators.pattern(/^\d{2}\/\d{2}$/)]],
      cvv: ['', [Validators.required, Validators.pattern(/^\d{3}$/)]]
    });
  }

  loadCartData(): void {
    const checkoutCartData = localStorage.getItem('checkoutCart');
    if (checkoutCartData) {
      this.cartData = JSON.parse(checkoutCartData);
      this.orderItems = this.cartData.items.map((item: any) => ({
        productId: item.productId,
        productName: item.name,
        quantity: item.quantity,
        unitPrice: item.price
      }));
    } else {
      // Redirect to cart if no checkout data
      this.router.navigate(['/cart']);
    }
  }

  updateBillingFromShipping(): void {
    if (this.sameAsBilling && this.shippingForm.valid) {
      this.billingForm.patchValue({
        firstName: this.shippingForm.get('firstName')?.value,
        lastName: this.shippingForm.get('lastName')?.value,
        addressLine1: this.shippingForm.get('addressLine1')?.value,
        addressLine2: this.shippingForm.get('addressLine2')?.value,
        city: this.shippingForm.get('city')?.value,
        state: this.shippingForm.get('state')?.value,
        zipCode: this.shippingForm.get('zipCode')?.value,
        country: this.shippingForm.get('country')?.value
      });
    }
  }

  goToStep(step: number): void {
    if (step < this.currentStep) {
      this.currentStep = step;
    } else if (step === 2 && this.shippingForm.valid) {
      this.currentStep = 2;
      this.updateBillingFromShipping();
    } else if (step === 3 && this.shippingForm.valid && this.billingForm.valid) {
      this.currentStep = 3;
    }
  }

  nextStep(): void {
    if (this.currentStep === 1 && this.shippingForm.valid) {
      this.currentStep = 2;
      this.updateBillingFromShipping();
    } else if (this.currentStep === 2 && this.billingForm.valid) {
      this.currentStep = 3;
    } else if (this.currentStep === 1) {
      this.markFormGroupTouched(this.shippingForm);
    } else if (this.currentStep === 2) {
      this.markFormGroupTouched(this.billingForm);
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  placeOrder(): void {
    this.orderSubmitted = true;
    
    if (this.paymentForm.invalid) {
      this.markFormGroupTouched(this.paymentForm);
      return;
    }

    this.isProcessing = true;

    // Simulate payment processing
    setTimeout(() => {
      // Create order request
      const orderRequest: CreateOrderRequest = {
        customerId: 'CUSTOMER_' + Date.now(),
        lineItems: this.orderItems
      };

      // Place order via service
      this.orderService.placeOrder(orderRequest)
        .pipe(takeUntil(this.destroy$))
        .subscribe(
          (response: any) => {
            this.isProcessing = false;
            this.orderPlaced = true;
            this.orderId = response.id || 'ORD_' + Date.now();

            // Clear cart after successful order
            localStorage.removeItem('cart');
            localStorage.removeItem('checkoutCart');

            // Dispatch order action to store
            this.store.dispatch(OrderActions.placeOrderSuccess({
              order: response
            }));
          },
          (error: any) => {
            this.isProcessing = false;
            alert('Error placing order: ' + (error?.message || 'Unknown error'));
          }
        );
    }, 1500);
  }

  viewOrder(): void {
    if (this.orderId) {
      this.router.navigate(['/orders']);
    }
  }

  continueShopping(): void {
    this.router.navigate(['/products']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getFormError(form: FormGroup, field: string): string {
    const control = form.get(field);
    if (control && control.errors && (control.dirty || control.touched)) {
      if (control.errors['required']) return `${field} is required`;
      if (control.errors['minlength']) return `${field} must be at least ${control.errors['minlength'].requiredLength} characters`;
      if (control.errors['pattern']) return `${field} format is invalid`;
      if (control.errors['email']) return 'Invalid email address';
    }
    return '';
  }
}
