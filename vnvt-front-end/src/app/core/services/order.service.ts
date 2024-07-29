import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Order } from '../models';
import { MockDataService } from './mock-data.service';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  // private apiUrl = 'https://api.example.com/orders'; // Placeholder for real API URL

  constructor(private mockDataService: MockDataService) { } // Inject the mock data service

  getOrders(): Observable<Order[]> {
    return this.mockDataService.getOrders(); // Use mock data
  }

  getOrderById(id: number): Observable<Order> {
    return this.mockDataService.getOrderById(id); // Use mock data
  }

  createOrder(order: Order): Observable<Order> {
    return this.mockDataService.getOrders().pipe(map(orders => {
      orders.push(order);
      return order;
    }));
  }

  updateOrder(id: number, order: Order): Observable<Order> {
    return this.mockDataService.getOrderById(id).pipe(map(o => {
      o.orderDate = order.orderDate;
      o.status = order.status;
      o.items = order.items;
      o.totalAmount = order.totalAmount;
      return o;
    }));
  }

  deleteOrder(id: number): Observable<void> {
    return this.mockDataService.getOrders().pipe(map(orders => {
      const index = orders.findIndex(o => o.id === id);
      if (index !== -1) {
        orders.splice(index, 1);
      }
      return;
    }));
  }
}
