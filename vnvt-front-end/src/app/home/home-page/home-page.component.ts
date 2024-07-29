import { Component } from '@angular/core';
import { ProductService } from '../../core/services';
import { Product } from '../../core/models';
import { CommonModule } from '@angular/common';
import { CartProductComponent } from '../../cart-product/cart-product.component';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, CartProductComponent, MatIconModule],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {
  products: Product[] = [];

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.productService.getProducts().subscribe((data: Product[]) => {
      console.log("test")
      this.products = data;
    });
  }
  onRemoveItem(item: any) { }
}
