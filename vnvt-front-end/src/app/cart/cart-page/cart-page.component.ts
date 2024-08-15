import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { combineLatest, map, Observable } from 'rxjs';
import { CartItem } from '../../core/models';
import { AppConfirmService } from '../../core/services/app-confirm/app-confirm.service';
import { CartService } from '../../core/services/cart.service';
import { MetaService } from '../../core/services/meta.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [MatIconModule, FormsModule, CommonModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    FormsModule],
  templateUrl: './cart-page.component.html',
  styleUrl: './cart-page.component.scss'
})
export class CartPageComponent {
  cartItems$: Observable<CartItem[]>;
  totalPrice$: Observable<number>;
  couponCode: string = '';
  subtotal$!: Observable<number>;
  discount: number = 0; // Discount percentage

  constructor(private cartService: CartService,
    private metaService: MetaService,
    private confirmService: AppConfirmService, private router: Router) {
    this.cartItems$ = this.cartService.getCartItems();
    this.totalPrice$ = this.cartService.getTotal();
  }

  ngOnInit(): void {
    this.metaService.updateTitle('Your Cart - My E-Commerce App');
    this.metaService.updateFavicon('assets/icon.ico');
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

  updateQuantity(item: CartItem, quantity: number) {
    this.cartService.updateQuantity(item, quantity);
  }

  getSubtotal(): Observable<number> {
    return this.cartItems$.pipe(
      map(items => items.reduce((acc, item) => acc + item.price * item.quantity, 0))
    );
  }

  getTax(): Observable<number> {
    return this.getSubtotal().pipe(
      map(subtotal => subtotal * 0.1) // Example tax rate of 10%
    );
  }

  applyCoupon() {
    // Dummy discount application for demonstration; replace with actual logic
    if (this.couponCode === 'DISCOUNT10') {
      this.discount = 10; // 10% discount
    } else {
      this.discount = 0;
    }
  }

  getDiscountedTotal(): Observable<number> {
    return this.totalPrice$.pipe(
      map(total => total - (total * (this.discount / 100)))
    );
  }

  getTotal(): Observable<number> {
    return combineLatest([this.getSubtotal(), this.getTax()]).pipe(
      map(([subtotal, tax]) => subtotal + tax)
    );
  }

  checkout(): void {
    // Checkout logic here
  }

  clearCart() {
    this.cartService.clearCart();
  }


  proceedToCheckout(): void {
    this.router.navigate(['/checkout']);
  }

  increaseQuantity(item: CartItem) {
    this.cartService.updateQuantity(item, item.quantity + 1);
  }

  decreaseQuantity(item: CartItem) {
    if (item.quantity > 1) {
      this.cartService.updateQuantity(item, item.quantity - 1);
    }
  }
}
