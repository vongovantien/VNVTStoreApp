import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { CartItem, Product } from '../core/models';
import { MatIconModule } from '@angular/material/icon';
import { CartService } from '../core/services/cart.service';

@Component({
  selector: 'app-card-product',
  standalone: true,
  imports: [RouterModule, MatButtonModule, MatIconModule],
  templateUrl: './card-product.component.html',
  styleUrls: ['./card-product.component.scss']
})
export class CardProductComponent {
  @Input() product!: Product;

  constructor(private cartService: CartService) { }
  addToCart() {
    const cartItem: CartItem = {
      ...this.product,
      quantity: 1
    };
    this.cartService.addToCart(cartItem);
  }
}
