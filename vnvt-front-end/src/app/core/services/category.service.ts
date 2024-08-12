import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { Category } from '../models';
import { MockDataService } from './mock-data.service';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = `${environment.apiUrl}/categories`; // Thay thế bằng URL API thực tế của bạn

  constructor(private http: HttpClient) { } // Inject the mock data service

  getCategories(): Observable<Category[]> {
    return this.http.get<any>(`${this.apiUrl}`).pipe(
      map(response => response.data)
    );
  }
}
