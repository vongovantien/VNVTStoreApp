import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Product } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`; // Thay thế bằng URL API thực tế của bạn

  constructor(private http: HttpClient) { }

  getProducts(): Observable<Product[]> {
    return this.http.get<any>(`${this.apiUrl}`).pipe(
      map(response => response.data.items)
    );
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data)
    );
  }

  createProduct(product: Product): Observable<Product> {
    return this.http.post<any>(`${this.apiUrl}`, product).pipe(
      map(response => response.data)
    );
  }

  updateProduct(id: number, product: Product): Observable<Product> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, product).pipe(
      map(response => response.data)
    );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`).pipe(
      map(response => { })
    );
  }

  getTrendingProducts(): Observable<Product[]> {
    return this.http.get<any>(`${this.apiUrl}/trending`).pipe(
      map(response => response.data.items)
    );
  }

  getProductFilter(filters: any): Observable<Product[]> {
    return this.http.get<any>(`${this.apiUrl}`, { params: filters }).pipe(
      map(response => response.data.items)
    );
  }

  searchProducts(searchTerm: string): Observable<Product[]> {
    return this.http.get<any>(`${this.apiUrl}/search`, { params: { q: searchTerm } }).pipe(
      map(response => response.data.items)
    );
  }
}
