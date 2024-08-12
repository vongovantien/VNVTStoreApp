
import { inject, Injectable } from "@angular/core";
import {
  CanActivate,
  Router
} from "@angular/router";
import { AuthService } from "../core/services/auth.service";

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  router = inject(Router);
  authService = inject(AuthService);
  canActivate(route: any, state: any): boolean {
    const isLoggedIn = this.authService.isAuthenticated();
    const isLoginOrRegister = state.url === '/login' || state.url === '/register';

    if (isLoggedIn && isLoginOrRegister) {
      // Nếu người dùng đã đăng nhập và đang cố gắng truy cập trang login hoặc register
      this.router.navigate(['/']); // Hoặc trang chính mà bạn muốn điều hướng đến
      return false;
    }

    if (!isLoggedIn && !isLoginOrRegister) {
      // Nếu người dùng chưa đăng nhập và đang cố gắng truy cập trang yêu cầu đăng nhập
      this.router.navigate(['/login']);
      return false;
    }

    return true;
  }
}

