import { Routes } from '@angular/router';
import { HomePageComponent } from './home/home-page/home-page.component';
import { ProductListComponent } from './product/product-list/product-list.component';
import { ProductDetailComponent } from './product/product-detail/product-detail.component';
import { CartPageComponent } from './cart/cart-page/cart-page.component';
import { CheckoutPageComponent } from './checkout/checkout-page/checkout-page.component';
import { AdminDashboardComponent } from './admin/admin-dashboard/admin-dashboard.component';
import { adminRoutes } from './admin/admin.routes';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomePageComponent },
      { path: 'products', component: ProductListComponent },
      { path: 'products/:id', component: ProductDetailComponent },
      { path: 'cart', component: CartPageComponent },
      { path: 'checkout', component: CheckoutPageComponent }
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    children: adminRoutes
  },
  { path: '**', redirectTo: '' }
];
