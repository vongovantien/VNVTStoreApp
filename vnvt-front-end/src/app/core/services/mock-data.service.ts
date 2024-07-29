import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { faker } from '@faker-js/faker';
import { Product, Category, Order, User, OrderItem } from '../models';

@Injectable({
  providedIn: 'root'
})
export class MockDataService {

  constructor() { }

  generateFakeProducts(count: number): Product[] {
    const products: Product[] = [];
    for (let i = 0; i < count; i++) {
      products.push({
        id: faker.datatype.number(),
        name: faker.commerce.productName(),
        description: faker.commerce.productDescription(),
        price: parseFloat(faker.commerce.price()),
        imageUrl: faker.image.imageUrl(),
        categoryId: faker.datatype.number()
      });
    }
    return products;
  }

  generateFakeCategories(count: number): Category[] {
    const categories: Category[] = [];
    for (let i = 0; i < count; i++) {
      categories.push({
        id: faker.datatype.number(),
        name: faker.commerce.department()
      });
    }
    return categories;
  }

  generateFakeOrders(count: number): Order[] {
    const orders: Order[] = [];
    for (let i = 0; i < count; i++) {
      orders.push({
        id: faker.datatype.number(),
        orderDate: faker.date.recent(),
        status: faker.helpers.arrayElement(['Pending', 'Shipped', 'Delivered']),
        items: this.generateFakeOrderItems(faker.datatype.number({ min: 1, max: 5 })),
        totalAmount: parseFloat(faker.commerce.price())
      });
    }
    return orders;
  }

  generateFakeOrderItems(count: number): OrderItem[] {
    const orderItems: OrderItem[] = [];
    for (let i = 0; i < count; i++) {
      orderItems.push({
        product: {
          id: faker.datatype.number(),
          name: faker.commerce.productName(),
          description: faker.commerce.productDescription(),
          price: parseFloat(faker.commerce.price()),
          imageUrl: faker.image.imageUrl(),
          categoryId: faker.datatype.number()
        },
        quantity: faker.datatype.number({ min: 1, max: 10 }),
        unitPrice: parseFloat(faker.commerce.price())
      });
    }
    return orderItems;
  }

  generateFakeUsers(count: number): User[] {
    const users: User[] = [];
    for (let i = 0; i < count; i++) {
      users.push({
        id: faker.datatype.number(),
        name: faker.person.fullName(),
        email: faker.internet.email(),
        password: faker.internet.password(),
        role: faker.helpers.arrayElement(['Admin', 'Customer'])
      });
    }
    return users;
  }

  // Methods to return fake data as Observables
  getProducts(): Observable<Product[]> {
    return of(this.generateFakeProducts(10)).pipe(delay(500));
  }

  getProductById(id: number): Observable<Product> {
    const product = this.generateFakeProducts(1)[0];
    return of(product).pipe(delay(500));
  }

  getCategories(): Observable<Category[]> {
    return of(this.generateFakeCategories(5)).pipe(delay(500));
  }

  getCategoryById(id: number): Observable<Category> {
    const category = this.generateFakeCategories(1)[0];
    return of(category).pipe(delay(500));
  }

  getOrders(): Observable<Order[]> {
    return of(this.generateFakeOrders(10)).pipe(delay(500));
  }

  getOrderById(id: number): Observable<Order> {
    const order = this.generateFakeOrders(1)[0];
    return of(order).pipe(delay(500));
  }

  getUsers(): Observable<User[]> {
    return of(this.generateFakeUsers(10)).pipe(delay(500));
  }

  getUserById(id: number): Observable<User> {
    const user = this.generateFakeUsers(1)[0];
    return of(user).pipe(delay(500));
  }
}
