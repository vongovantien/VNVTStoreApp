import { Component, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CartItem, Order } from '../../core/models';
import { AppConfirmService } from '../../core/services/app-confirm/app-confirm.service';
import { CartService } from '../../core/services/cart.service';
import { map, Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { OrderService, ToastService } from '../../core/services';
import { ApiResponse } from '../../core/models/api-response.model';

@Component({
  selector: 'app-checkout-page',
  standalone: true,
  imports: [ReactiveFormsModule,
    CommonModule,
    MatStepperModule,
    MatFormFieldModule,
    MatTabsModule,
    MatInputModule,
    MatButtonModule],
  templateUrl: './checkout-page.component.html',
  styleUrl: './checkout-page.component.scss'
})
export class CheckoutPageComponent {
  addressForm!: FormGroup;
  cartItems$: Observable<CartItem[]>;
  shippingForm!: FormGroup;
  paymentForm!: FormGroup;
  @ViewChild('stepper') stepper!: MatStepper;
  totalPrice$: Observable<number>;
  constructor(private fb: FormBuilder,
    private confirmService: AppConfirmService,
    private cartService: CartService,
    private orderService: OrderService,
    private toastService: ToastService) {
    this.totalPrice$ = this.cartService.getTotal();
    this.cartItems$ = this.cartService.getCartItems();
  }

  ngOnInit(): void {
    this.addressForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      address: ['', Validators.required],
      apartment: [''],
      city: ['', Validators.required],
      country: ['', Validators.required],
      zipcode: ['', Validators.required],
    });

    this.shippingForm = this.fb.group({
      // Define your shipping form controls and validators here
    });

    this.paymentForm = this.fb.group({
      // Define your payment form controls and validators here
    });
  }

  onContinue(stepper: MatStepper): void {
    if (stepper.selectedIndex === 0 && this.addressForm.valid) {
      stepper.next(); // Move to the next step (Shipping)
    } else if (stepper.selectedIndex === 1 && this.shippingForm.valid) {
      stepper.next(); // Move to the next step (Payment)
    } else if (stepper.selectedIndex === 2 && this.paymentForm.valid) {
      this.onSubmit();
    } else {
      if (stepper.selectedIndex === 0) {
        this.addressForm.markAllAsTouched(); // Show validation errors
      } else if (stepper.selectedIndex === 1) {
        this.shippingForm.markAllAsTouched(); // Show validation errors
      } else if (stepper.selectedIndex === 2) {
        this.paymentForm.markAllAsTouched(); // Show validation errors
      }
    }
  }

  onSubmit() {
    if (this.addressForm.valid && this.shippingForm.valid && this.paymentForm.valid) {
      const data: Order = {
        id: 0,
        userId: 31, // Ensure this is provided from your user session or similar
        orderStatus: 'Pending', // Or appropriate status
        totalAmount: 0, // Ensure this is calculated and available
        firstName: this.addressForm.value.firstName,
        lastName: this.addressForm.value.lastName,
        address: this.addressForm.value.address,
        apartment: this.addressForm.value.apartment,
        city: this.addressForm.value.city,
        country: this.addressForm.value.country,
        zipcode: this.addressForm.value.zipcode,
        shippingMethod: this.shippingForm.value.method || 'null', // Adjust according to your form
        orderItems: [],
      };

      this.orderService.createOrder(data).subscribe((response: ApiResponse<any>) => {
        if (response.success)
          this.cartService.clearCart();
        return this.toastService.showToast(response.message, 'success');
        // Handle successful checkout
      }, error => {
        console.log('Checkout error:', error);
        // Handle error
      });
    } else {
      console.log('Some forms are invalid. Please complete all required fields.');
    }
  }

  removeItem(item: CartItem) {
    this.confirmService.confirm({
      title: 'Delete Item',
      message: `Are you sure you want to delete "${item.name}" from the cart?`
    }).subscribe(result => {
      if (result) {
        this.cartService.removeFromCart(item.id);
      }
    });
  }

  getDiscountedTotal(): Observable<number> {
    return this.totalPrice$.pipe(
      map(total => total - (total))
    );
  }
}
