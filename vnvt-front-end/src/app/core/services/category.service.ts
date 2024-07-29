import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Category } from '../models';
import { MockDataService } from './mock-data.service';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  // private apiUrl = 'https://api.example.com/categories'; // Placeholder for real API URL

  constructor(private mockDataService: MockDataService) { } // Inject the mock data service

  getCategories(): Observable<Category[]> {
    return this.mockDataService.getCategories(); // Use mock data
  }

  getCategoryById(id: number): Observable<Category> {
    return this.mockDataService.getCategoryById(id); // Use mock data
  }

  createCategory(category: Category): Observable<Category> {
    return this.mockDataService.getCategories().pipe(map(categories => {
      categories.push(category);
      return category;
    }));
  }

  updateCategory(id: number, category: Category): Observable<Category> {
    return this.mockDataService.getCategoryById(id).pipe(map(c => {
      c.name = category.name;
      return c;
    }));
  }

  deleteCategory(id: number): Observable<void> {
    return this.mockDataService.getCategories().pipe(map(categories => {
      const index = categories.findIndex(c => c.id === id);
      if (index !== -1) {
        categories.splice(index, 1);
      }
      return;
    }));
  }
}
