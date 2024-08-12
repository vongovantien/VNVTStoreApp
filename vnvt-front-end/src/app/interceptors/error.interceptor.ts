import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastService } from '../core/services/toast.service'; // Adjust path as needed
import { ApiResponse } from '../core/models/api-response.model'; // Adjust path as needed

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private toastService: ToastService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let apiResponse: ApiResponse<any>;

        // Check if the error has an object structure that matches ApiResponse
        if (error.error && typeof error.error === 'object' && error.error.Message) {
          apiResponse = error.error as ApiResponse<any>;
        } else {
          // If the error does not match the ApiResponse structure, create a default response
          apiResponse = {
            success: false,
            message: 'An unexpected error occurred',
            data: null,
            statusCode: error.status
          };
        }

        // Show a toast notification with the error message
        this.toastService.showToast(apiResponse.message, 'error');

        // Return the error as an observable with a custom message
        return throwError(() => new Error(apiResponse.message));
      })
    );
  }
}
