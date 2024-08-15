import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgxSkeletonLoaderModule } from 'ngx-skeleton-loader';
import { of } from 'rxjs';
import { CardProductComponent } from '../../card-product/card-product.component';
import { Product } from '../../core/models';
import { CategoryService, ToastService } from '../../core/services';
import { AppLoaderComponent } from '../../core/services/app-loader/app-loader.component';
import { ProductService } from '../../core/services/product.service';
import { ErrorInterceptor } from '../../interceptors/error.interceptor';
import { ProductListComponent } from './product-list.component';

describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;
  let productService: ProductService;
  let categoryService: CategoryService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        FormsModule,
        ReactiveFormsModule,
        MatSelectModule,
        MatCheckboxModule,
        MatIconModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        TranslateModule.forRoot(),
        NgxSkeletonLoaderModule,
        AppLoaderComponent,
        CardProductComponent
      ],
      declarations: [ProductListComponent],
      providers: [
        ProductService,
        CategoryService,
        ToastService,
        { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
        TranslateService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
    productService = TestBed.inject(ProductService);
    categoryService = TestBed.inject(CategoryService);

    spyOn(productService, 'getProductFilter').and.returnValue(of([]));
    spyOn(categoryService, 'getCategories').and.returnValue(of([]));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize categories and products on ngOnInit', () => {
    const categories = [{ id: 1, name: 'Category 1' }];
    const products: Product[] = [{
      id: 1,
      name: 'Product 1',
      createdDate: '2024-01-01',
      updatedDate: '2024-01-01',
      categoryName: 'Category 1',
      description: 'Product 1 description',
      price: 100,
      categoryId: 1,
      stockQuantity: 10,
      productImages: [{ id: 1, createdDate: '2024-01-01', updatedDate: '2024-01-01', imageUrl: 'http://example.com/image.jpg', productId: 1 }],
      imageUrl: 'http://example.com/image.jpg'
    }];

    spyOn(categoryService, 'getCategories').and.returnValue(of(categories));
    spyOn(productService, 'getProductFilter').and.returnValue(of(products));

    component.ngOnInit();

    expect(categoryService.getCategories).toHaveBeenCalled();
    expect(productService.getProductFilter).toHaveBeenCalledWith(component.filters, component.categoryId);
    expect(component.categories).toEqual(categories);
    expect(component.products).toEqual(products);
  });

  it('should load categories', () => {
    const categories = [{ id: 1, name: 'Category 1' }];
    spyOn(categoryService, 'getCategories').and.returnValue(of(categories));

    component.loadCategories();

    expect(component.categories).toEqual(categories);
  });

  it('should load products', () => {
    const products: Product[] = [{
      id: 1,
      name: 'Product 1',
      createdDate: '2024-01-01',
      updatedDate: '2024-01-01',
      categoryName: 'Category 1',
      description: 'Product 1 description',
      price: 100,
      categoryId: 1,
      stockQuantity: 10,
      productImages: [{ id: 1, createdDate: '2024-01-01', updatedDate: '2024-01-01', imageUrl: 'http://example.com/image.jpg', productId: 1 }],
      imageUrl: 'http://example.com/image.jpg'
    }];
    spyOn(productService, 'getProductFilter').and.returnValue(of(products));

    component.loadProducts();

    expect(component.products).toEqual(products);
  });

  it('should handle search input', () => {
    const searchTerm = 'test';
    spyOn(productService, 'getProductFilter').and.returnValue(of([]));

    component.onSearch({ target: { value: searchTerm } });

    component.searchTerm$.subscribe(value => {
      expect(value).toBe(searchTerm);
    });
    expect(productService.getProductFilter).toHaveBeenCalledWith(component.filters, component.categoryId);
  });

  it('should clear search and reload products', () => {
    spyOn(component, 'loadProducts');
    component.clearSearch();

    expect(component.filters.Keyword).toBe('');
    expect(component.filters.PageNumber).toBe(1);
    expect(component.loadProducts).toHaveBeenCalled();
  });

  it('should clear all filters and reload products', () => {
    spyOn(component, 'loadProducts');
    component.clearFilters();

    expect(component.filters).toEqual({
      PageNumber: 1,
      PageSize: 10,
      Keyword: '',
      SortField: '',
      SortDescending: false
    });
    expect(component.categoryId).toBe(0);
    expect(component.loadProducts).toHaveBeenCalled();
  });

  it('should sort products correctly', () => {
    spyOn(component, 'loadProducts');
    component.sortOption = 'priceAsc';
    component.sortProducts();

    expect(component.filters.SortField).toBe('price');
    expect(component.filters.SortDescending).toBe(false);
    expect(component.loadProducts).toHaveBeenCalled();
  });

  it('should handle category change', () => {
    spyOn(component, 'loadProducts');
    component.onCategoryChange();

    expect(component.loadProducts).toHaveBeenCalled();
  });
});
