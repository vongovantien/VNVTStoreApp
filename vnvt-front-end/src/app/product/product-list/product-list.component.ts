import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../core/services/product.service';
import { Product } from '../../core/models/product.model';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { FormsModule } from '@angular/forms';
import { CardProductComponent } from '../../card-product/card-product.component';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    CardProductComponent
  ],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  categories: string[] = ['Electronics', 'Books', 'Clothing']; // Example categories
  filters = {
    inStock: false,
    freeShipping: false,
    category: ''
  };
  sortOption: string = 'popular';

  constructor(private productService: ProductService) { }

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.productService.getProductFilter(this.filters).subscribe(products => {
      this.products = products;
    });
  }

  onSearch(event: any) {
    this.productService.searchProducts(event.target.value).subscribe(products => {
      this.products = products;
    });
  }

  clearSearch() {
    this.onSearch('');
  }

  clearFilters() {
    this.filters = {
      inStock: false,
      freeShipping: false,
      category: ''
    };
    this.loadProducts();
  }

  sortProducts() {
    if (this.sortOption === 'price') {
      this.products.sort((a, b) => a.price - b.price);
    } else if (this.sortOption === 'new') {
      this.products.sort((a, b) => b.id - a.id); // Assuming higher ID means newer product
    } else {
      // Default to popular, no specific action needed
    }
  }

  loadMore() {
    // Implement logic to load more products if available
    // This could involve updating the filters or pagination settings in the service call
    console.log('Load more products');
  }
}
