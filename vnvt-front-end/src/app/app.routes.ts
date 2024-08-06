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

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomePageComponent },
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'products', component: ProductListComponent },
      { path: 'products/:id', component: ProductDetailComponent },
      { path: 'about', component: AboutPageComponent },
      { path: 'cart', component: CartPageComponent },
      { path: 'checkout', component: CheckoutPageComponent },
      { path: 'profile', component: ProfilePageComponent}
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    children: adminRoutes
  },
  { path: '**', redirectTo: '' }
];
