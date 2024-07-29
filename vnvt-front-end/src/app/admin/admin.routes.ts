import { Routes } from '@angular/router';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { AdminProductsComponent } from './admin-products/admin-products.component';
import { AdminOrdersComponent } from './admin-orders/admin-orders.component';
import { AdminUsersComponent } from './admin-users/admin-users.component';

export const adminRoutes: Routes = [
  { path: '', component: AdminDashboardComponent },
  { path: 'products', component: AdminProductsComponent },
  { path: 'orders', component: AdminOrdersComponent },
  { path: 'users', component: AdminUsersComponent },
];
