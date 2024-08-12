import { Injectable } from "@angular/core";
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpErrorResponse
} from "@angular/common/http";
import { catchError, Observable, switchMap, throwError } from "rxjs";
import { AuthService } from "../core/services/auth.service";
@Injectable()
export class TokenInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    let changedReq = req;

    if (token) {
      changedReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        },
      });
    }

    return next.handle(changedReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !this.authService.isTokenExpired()) {
          // Token is expired, try to refresh
          return this.authService.refreshToken().pipe(
            switchMap(newToken => {
              if (newToken) {
                // Retry the failed request with the new token
                const retryReq = req.clone({
                  setHeaders: {
                    Authorization: `Bearer ${newToken}`
                  }
                });
                return next.handle(retryReq);
              } else {
                // Handle refresh token failure
                this.authService.logout();
                return throwError(error);
              }
            }),
            catchError(err => {
              this.authService.logout();
              return throwError(err);
            })
          );
        } else {
          // If not token expired or another error, just throw the original error
          return throwError(error);
        }
      })
    );
  }
}
