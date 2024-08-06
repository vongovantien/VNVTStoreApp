import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { PagingParameters, Product } from '../models';
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

  getProductFilter(pagingParams: PagingParameters): Observable<Product[]> {
    let params = new HttpParams()
      .set('pageNumber', pagingParams.PageNumber.toString())
      .set('pageSize', pagingParams.PageSize.toString())
      .set('keyword', pagingParams.Keyword || '')
      .set('sortField', pagingParams.SortField || '')
      .set('sortDescending', pagingParams.SortDescending.toString());

    return this.http.get<any>(`${this.apiUrl}/paging`, { params }).pipe(
      map(response => response.data.items)
    );
  }

  searchProducts(searchTerm: string): Observable<Product[]> {
    return this.http.get<any>(`${this.apiUrl}/search`, { params: { q: searchTerm } }).pipe(
      map(response => response.data.items)
    );
  }
}
