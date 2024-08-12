import { Routes } from '@angular/router';
import { AboutPageComponent } from './about/about-page/about-page.component';
import { adminRoutes } from './admin/admin.routes';
import { CartPageComponent } from './cart/cart-page/cart-page.component';
import { CheckoutPageComponent } from './checkout/checkout-page/checkout-page.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { HomePageComponent } from './home/home-page/home-page.component';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { ProductDetailComponent } from './product/product-detail/product-detail.component';
import { ProductListComponent } from './product/product-list/product-list.component';
import { ProfilePageComponent } from './profile/profile-page/profile-page.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomePageComponent },
      { path: 'login', component: LoginComponent, canActivate: [AuthGuard] },
      { path: 'register', component: RegisterComponent, canActivate: [AuthGuard]  },
      { path: 'products', component: ProductListComponent },
      { path: 'products/:id', component: ProductDetailComponent },
      { path: 'about', component: AboutPageComponent },
      { path: 'cart', component: CartPageComponent },
      { path: 'checkout', component: CheckoutPageComponent, canActivate: [AuthGuard] },
      { path: 'profile', component: ProfilePageComponent, canActivate: [AuthGuard]},
      { path: 'forgot-password', component: ForgotPasswordComponent },
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    children: adminRoutes
  },
  { path: '**', redirectTo: '' }
];
