import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Product } from '../models';
import { MockDataService } from './mock-data.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  // private apiUrl = 'https://api.example.com/products'; // Placeholder for real API URL

  constructor(private mockDataService: MockDataService) { } // Inject the mock data service

  getProducts(): Observable<Product[]> {
    return this.mockDataService.getProducts(); // Use mock data
  }

  getProductById(id: number): Observable<Product> {
    return this.mockDataService.getProductById(id); // Use mock data
  }

  createProduct(product: Product): Observable<Product> {
    return this.mockDataService.getProducts().pipe(map(products => {
      products.push(product);
      return product;
    }));
  }

  updateProduct(id: number, product: Product): Observable<Product> {
    return this.mockDataService.getProductById(id).pipe(map(p => {
      p.name = product.name;
      p.description = product.description;
      p.price = product.price;
      p.imageUrl = product.imageUrl;
      return p;
    }));
  }

  deleteProduct(id: number): Observable<void> {
    return this.mockDataService.getProducts().pipe(map(products => {
      const index = products.findIndex(p => p.id === id);
      if (index !== -1) {
        products.splice(index, 1);
      }
      return;
    }));
  }
}
