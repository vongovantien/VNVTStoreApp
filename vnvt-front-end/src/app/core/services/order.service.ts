import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Order } from '../models';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`; // Use actual API URL from environment

  constructor(private http: HttpClient) { }

  // Fetch all orders
  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl).pipe(
      map(response => response) // Adjust according to your API response structure
    );
  }

  // Fetch a single order by ID
  getOrderById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${id}`).pipe(
      map(response => response) // Adjust according to your API response structure
    );
  }

  // Create a new order
  createOrder(order: Order): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(this.apiUrl, order).pipe(
      map(response => response) // Adjust according to your API response structure
    );
  }

  // Update an existing order by ID
  updateOrder(id: number, order: Order): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, order).pipe(
      map(response => response) // Adjust according to your API response structure
    );
  }

  // Delete an order by ID
  deleteOrder(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      map(response => response) // Adjust according to your API response structure
    );
  }
}
