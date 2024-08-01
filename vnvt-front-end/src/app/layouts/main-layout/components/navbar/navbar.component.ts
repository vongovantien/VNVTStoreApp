import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MenuItem } from '../../../../core/models/menu-items.model';

export const NAVBAR_ITEMS: MenuItem[] = [
  { name: 'NAVBAR.HOME', route: '/', icon: 'home' },
  { name: 'NAVBAR.PRODUCTS', route: '/products', icon: 'shopping_bag' },
  { name: 'NAVBAR.CATEGORIES', route: '/categories', icon: 'category' },
  { name: 'NAVBAR.ABOUT', route: '/about', icon: 'info' },
  {
    name: 'NAVBAR.ACCOUNT',
    icon: 'account_circle',
    subItems: [
      { name: 'NAVBAR.PROFILE', route: '/profile', icon: 'person' },
      { name: 'NAVBAR.ORDERS', route: '/orders', icon: 'list_alt' },
      { name: 'NAVBAR.LOGOUT', route: '/logout', icon: 'logout' }
    ]
  },
  {
    name: 'NAVBAR.ADMIN',
    icon: 'admin_panel_settings',
    subItems: [
      { name: 'NAVBAR.USERS', route: '/admin/users', icon: 'people' },
      { name: 'NAVBAR.REPORTS', route: '/admin/reports', icon: 'bar_chart' }
    ]
  }
];


@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    RouterModule,
    TranslateModule
  ]
})
export class NavbarComponent {

  navbarItems: MenuItem[] = NAVBAR_ITEMS;

  constructor(private translate: TranslateService) {
    this.translate.addLangs(['en', 'vi']);
    this.translate.setDefaultLang('en');
  }

  switchLang(lang: string) {
    this.translate.use(lang);
  }
}
