import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, map, Observable } from 'rxjs';
import { CartItem } from '../models/cart-item.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItemsSubject = new BehaviorSubject<CartItem[]>([]);
  cartItems$ = this.cartItemsSubject.asObservable();

  getCartItems(): Observable<CartItem[]> {
    return this.cartItems$;
  }

  addItem(item: CartItem) {
    const currentItems = this.cartItemsSubject.getValue();
    this.cartItemsSubject.next([...currentItems, item]);
  }

  removeItem(item: CartItem) {
    const currentItems = this.cartItemsSubject.getValue().filter(i => i !== item);
    this.cartItemsSubject.next(currentItems);
  }

  updateQuantity(item: CartItem, quantity: number) {
    const currentItems = this.cartItemsSubject.getValue().map(i =>
      i === item ? { ...i, quantity } : i
    );
    this.cartItemsSubject.next(currentItems);
  }

  clearCart() {
    this.cartItemsSubject.next([]);
  }

  getSubtotal(): Observable<number> {
    return this.cartItems$.pipe(
      map(items => items.reduce((sum, item) => sum + item.price * item.quantity, 0))
    );
  }

  getTax(): Observable<number> {
    return this.getSubtotal().pipe(map(subtotal => subtotal * 0.1)); // Assuming 10% tax
  }

  getTotal(): Observable<number> {
    return combineLatest([this.getSubtotal(), this.getTax()]).pipe(
      map(([subtotal, tax]) => subtotal + tax)
    );
  }

  checkout() {
    // Implement checkout functionality here
    alert('Proceeding to checkout');
    this.clearCart();
  }
}
