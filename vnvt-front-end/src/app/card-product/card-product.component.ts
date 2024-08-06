import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { Product } from '../core/models';

@Component({
  selector: 'app-card-product',
  standalone: true,
  imports: [RouterModule, MatButtonModule],
  templateUrl: './card-product.component.html',
  styleUrls: ['./card-product.component.scss']
})
export class CardProductComponent {
  @Input() product!: Product;
  @Output() remove = new EventEmitter<void>();

  removeFromCart() {
    this.remove.emit();
  }
}
