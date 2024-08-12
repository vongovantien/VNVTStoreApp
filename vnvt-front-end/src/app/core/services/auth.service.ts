import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { TokenInterceptor } from './../../interceptors/token.interceptor';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private tokenKey = 'authToken'; // Ensure consistency

  constructor(private http: HttpClient, private router: Router) { }

  login(username: string, password: string, rememberMe: boolean): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/login`, { username, password, rememberMe })
      .pipe(
        map(response => {
          this.storeToken(response.data.token, rememberMe); // Adjust token field as needed
          return response;
        })
      );
  }

  register(user: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, user);
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const decodedToken = JSON.parse(atob(token.split('.')[1]));
      const exp = decodedToken.exp * 1000;
      return Date.now() > exp;
    } catch (error) {
      return true;
    }
  }

  isAuthenticated(): boolean {
    return !!(localStorage.getItem(this.tokenKey) || sessionStorage.getItem(this.tokenKey));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.tokenKey);
    this.router.navigate(['/login']); // Redirect to login page or other actions
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey) || sessionStorage.getItem(this.tokenKey);
  }

  refreshToken(): Observable<string> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/refresh-token`, {})
      .pipe(
        map(response => {
          this.storeToken(response.token, this.isTokenPersisted());
          return response.token;
        }),
        catchError(err => {
          this.logout();
          return throwError(err);
        })
      );
  }

  private storeToken(token: string, rememberMe: boolean): void {
    console.log(token)
    console.log(rememberMe)
    if (rememberMe) {
      localStorage.setItem(this.tokenKey, token);
    } else {
      sessionStorage.setItem(this.tokenKey, token);
    }
  }

  private isTokenPersisted(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }
}
