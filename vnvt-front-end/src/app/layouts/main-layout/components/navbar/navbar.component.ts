import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
export interface NavbarItem {
  name: string;
  icon?: string;
  route: string;
  submenu?: NavbarItem[];
}

export const NAVBAR_ITEMS: NavbarItem[] = [
  { name: 'Home', route: '/' },
  { name: 'Products', route: '/products' },
  { name: 'Categories', route: '/categories' },
  { name: 'Cart', route: '/cart' },
  {
    name: 'Account',
    icon: 'account_circle',
    route: '',
    submenu: [
      { name: 'Profile', route: '/profile' },
      { name: 'Orders', route: '/orders' },
      { name: 'Logout', route: '' } // Define the logout action in your component
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

  navbarItems: NavbarItem[] = NAVBAR_ITEMS;

  constructor(private translate: TranslateService) {
    this.translate.addLangs(['en', 'fr']);
    this.translate.setDefaultLang('en');
  }

  switchLang(lang: string) {
    this.translate.use(lang);
  }
}
