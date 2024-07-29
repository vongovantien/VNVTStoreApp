import { Component } from '@angular/core';
import { ProductService } from '../../core/services';
import { Product } from '../../core/models';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule],
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
}
