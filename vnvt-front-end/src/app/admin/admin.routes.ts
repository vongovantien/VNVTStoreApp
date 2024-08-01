import { Routes } from '@angular/router';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { AdminOrdersComponent } from './admin-orders/admin-orders.component';
import { AdminProductsComponent } from './admin-products/admin-products.component';
import { ProductCreateComponent } from './admin-products/components/product-create/product-create.component';
import { ProductEditComponent } from './admin-products/components/product-edit/product-edit.component';
import { AdminUsersComponent } from './admin-users/admin-users.component';
export const adminRoutes: Routes = [
  { path: '', component: AdminDashboardComponent },
  { path: 'products', component: AdminProductsComponent },
  { path: 'products/create', component: ProductCreateComponent },
  { path: 'products/edit/:id', component: ProductEditComponent },
  { path: 'orders', component: AdminOrdersComponent },
  { path: 'users', component: AdminUsersComponent },
];
