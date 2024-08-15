import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, map, Observable } from 'rxjs';
import { CartItem } from '../models/cart-item.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private localStorageKey = 'cartItems'; // Key for localStorage
  private cartItemsSubject = new BehaviorSubject<CartItem[]>(this.getCartItemsFromLocalStorage());
  cartItems$ = this.cartItemsSubject.asObservable();

  constructor() {
    // Initialize the cart from localStorage
    const cartItems = this.getCartItemsFromLocalStorage();
    this.cartItemsSubject.next(cartItems);
  }

  private getCartItemsFromLocalStorage(): CartItem[] {
    const storedItems = localStorage.getItem(this.localStorageKey);
    return storedItems ? JSON.parse(storedItems) : [];
  }

  private saveCartItemsToLocalStorage(items: CartItem[]) {
    localStorage.setItem(this.localStorageKey, JSON.stringify(items));
  }

  getCartItems(): Observable<CartItem[]> {
    return this.cartItems$;
  }

  addToCart(item: CartItem) {
    const currentItems = this.cartItemsSubject.getValue();
    const existingItemIndex = currentItems.findIndex(cartItem => cartItem.id === item.id);

    if (existingItemIndex > -1) {
      // Item already exists, update quantity if needed
      currentItems[existingItemIndex].quantity += item.quantity;
    } else {
      // Item does not exist, add new item
      currentItems.push(item);
    }

    this.cartItemsSubject.next(currentItems);
    this.saveCartItemsToLocalStorage(currentItems); // Save to localStorage
  }

  removeFromCart(itemId: number) {
    const updatedItems = this.cartItemsSubject.getValue().filter(item => item.id !== itemId);
    this.cartItemsSubject.next(updatedItems);
    this.saveCartItemsToLocalStorage(updatedItems); // Save to localStorage
  }

  updateQuantity(item: CartItem, quantity: number) {
    const currentItems = this.cartItemsSubject.getValue().map(i =>
      i.id === item.id ? { ...i, quantity } : i
    );
    this.cartItemsSubject.next(currentItems);
    this.saveCartItemsToLocalStorage(currentItems); // Save to localStorage
  }

  clearCart() {
    this.cartItemsSubject.next([]);
    localStorage.removeItem(this.localStorageKey); // Clear from localStorage
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

  getTotalQuantity(): Observable<number> {
    return this.cartItems$.pipe(
      map(items => items.reduce((total, item) => total + item.quantity, 0))
    );
  }

  checkout() {
    // Implement checkout functionality here
    alert('Proceeding to checkout');
    this.clearCart();
  }
}
