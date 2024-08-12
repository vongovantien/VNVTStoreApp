import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CardProductComponent } from '../../card-product/card-product.component';
import { Category, PagingParameters } from '../../core/models';
import { Product } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CategoryService, ToastService } from '../../core/services';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ErrorInterceptor } from '../../interceptors/error.interceptor';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';

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
    CardProductComponent,
    TranslateModule,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    ToastService,
  ],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  categories: Category[] = []; // Example categories
  categoryId!: number;
  filters: PagingParameters = {
    PageNumber: 1,
    PageSize: 10,
    Keyword: '',
    SortField: '',
    SortDescending: false
  };
  sortOption: string = 'popular';
  searchControl = new FormControl();
  constructor(private productService: ProductService,
    private translate: TranslateService,
    private categoryService: CategoryService) {
    translate.setDefaultLang('en');
  }

  searchTerm$ = new Subject<string>();

  ngOnInit(): void {
    this.searchTerm$.pipe(
      debounceTime(300),  // Wait for 300ms pause in events
      distinctUntilChanged(),  // Only emit when the current value is different than the last
      switchMap(term => {
        this.filters.PageNumber = 1; // Reset page number for new search
        this.filters.Keyword = term;
        return this.productService.getProductFilter(this.filters, this.categoryId);
      })
    ).subscribe((data: any) => {
      this.products = data;
    });

    this.loadCategories();
    this.loadProducts();
  }

  loadCategories() {
    this.categoryService.getCategories().subscribe(cate => {
      console.log(cate)
      this.categories = cate;
    });
  }


  loadProducts() {
    this.productService.getProductFilter(this.filters, this.categoryId).subscribe(products => {
      this.products = products;
    });
  }

  onSearch(event: any): void {
    const searchTerm = event.target.value;
    this.searchTerm$.next(searchTerm);  // Push the search term to the Subject
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
    this.categoryId = 0;
    this.loadProducts();
  }
  sortProducts() {
    switch (this.sortOption) {
      case 'priceAsc':
        this.filters.SortField = 'price';
        this.filters.SortDescending = false; // Sắp xếp theo giá tăng dần
        break;
      case 'priceDesc':
        this.filters.SortField = 'price';
        this.filters.SortDescending = true; // Sắp xếp theo giá giảm dần
        break;
      case 'ratingHigh':
        this.filters.SortField = 'rating';
        this.filters.SortDescending = true; // Sản phẩm được đánh giá cao nhất
        break;
      case 'ratingMost':
        this.filters.SortField = 'ratingCount';
        this.filters.SortDescending = true; // Sản phẩm được đánh giá nhiều nhất
        break;
      case 'salesMost':
        this.filters.SortField = 'sales';
        this.filters.SortDescending = true; // Sản phẩm bán chạy nhất
        break;
      case 'newest':
        this.filters.SortField = 'id'; // Assuming higher ID means newer product
        this.filters.SortDescending = true;// Sản phẩm mới nhất
        break;
      // case 'discountHigh':
      //   this.filters.SortField = 'discount';
      //   this.filters.SortDescending = true; // Sản phẩm có mức giảm giá cao nhất
      //   break;
      // case 'discountAvailable':
      //   this.filters.SortField = 'hasDiscount';
      //   this.filters.SortDescending = true; // Sản phẩm có giảm giá
      //   break;
      // case 'popular':
      //   this.filters.SortField = 'popularity';
      //   this.filters.SortDescending = true; // Sản phẩm phổ biến
      //   break;
      // case 'brand':
      //   this.filters.SortField = 'brand';
      //   this.filters.SortDescending = false; // Sản phẩm từ các thương hiệu được ưa chuộng
      //   break;
      default:
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

  onCategoryChange() {
    this.loadProducts()
  }
}
