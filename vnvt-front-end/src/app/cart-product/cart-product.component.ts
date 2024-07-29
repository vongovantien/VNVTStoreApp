import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-cart-product',
  standalone: true,
  templateUrl: './cart-product.component.html',
  styleUrls: ['./cart-product.component.scss']
})
export class CartProductComponent {
  @Input() product: any;
  @Output() remove = new EventEmitter<void>();

  removeFromCart() {
    this.remove.emit();
  }
}
