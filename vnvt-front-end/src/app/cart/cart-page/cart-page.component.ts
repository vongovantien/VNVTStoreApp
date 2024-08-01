import { Component } from '@angular/core';
import { CartService } from '../../core/services/cart.service';
import { CartItem } from '../../core/models';
import { combineLatest, map, Observable } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MetaService } from '../../core/services/meta.service';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

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
  couponCode: string = '';

  constructor(private cartService: CartService, private metaService: MetaService) {
    this.addSampleData();
    this.cartItems$ = this.cartService.getCartItems();
  }
  addSampleData() {
    const sampleItems: CartItem[] = [
      { id: 1, name: 'Product 1', price: 100, quantity: 1, imageUrl: 'https://via.placeholder.com/100' },
      { id: 2, name: 'Product 2', price: 200, quantity: 2, imageUrl: 'https://via.placeholder.com/100' }
    ];
    sampleItems.forEach(item => this.cartService.addItem(item));
  }
  ngOnInit(): void {
    this.metaService.updateTitle('Your Cart - My E-Commerce App');
    this.metaService.updateFavicon('assets/icon.ico');
  }
  applyCoupon(): void {
    // Logic to apply coupon
  }
  removeItem(item: CartItem) {
    this.cartService.removeItem(item);
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
    // Logic to proceed to checkout
  }
}
