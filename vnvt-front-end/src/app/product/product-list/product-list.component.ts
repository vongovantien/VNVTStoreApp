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
import { PagingParameters } from '../../core/models';

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
  filters: PagingParameters = {
    PageNumber: 1,
    PageSize: 10,
    Keyword: '',
    SortField: '',
    SortDescending: false
  };
  sortOption: string = 'popular';

  constructor(private productService: ProductService) { }

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.productService.getProductFilter(this.filters).subscribe(products => {
      console.log(products)
      this.products = [...this.products, ...products];
    });
  }

  onSearch(event: any) {
    this.filters.PageNumber = 1; // Reset page number for new search
    this.filters.Keyword = event.target.value;
    this.loadProducts();
  }

  clearSearch() {
    this.filters.Keyword = '';
    this.filters.PageNumber = 1;
    this.loadProducts();
  }

  clearFilters() {
    this.filters = {
      PageNumber: 1,
      PageSize: 10,
      Keyword: '',
      SortField: '',
      SortDescending: false
    };
    this.loadProducts();
  }
  sortProducts() {
    if (this.sortOption === 'price') {
      this.filters.SortField = 'price';
      this.filters.SortDescending = false; // Example sorting criteria
    } else if (this.sortOption === 'new') {
      this.filters.SortField = 'id'; // Assuming higher ID means newer product
      this.filters.SortDescending = true;
    } else {
      this.filters.SortField = '';
      this.filters.SortDescending = false;
    }
    this.loadProducts();
  }

  loadMore() {
    // Implement logic to load more products if available
    // This could involve updating the filters or pagination settings in the service call
    console.log('Load more products');
  }
}
