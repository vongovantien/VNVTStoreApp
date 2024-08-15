import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MenuItem } from '../../../../core/models/menu-items.model';
import { AuthService } from '../../../../core/services/auth.service';
import { CartService } from '../../../../core/services/cart.service';

export const NAVBAR_ITEMS: MenuItem[] = [
  { name: 'NAVBAR.HOME', route: '/', icon: 'home' },
  { name: 'NAVBAR.PRODUCTS', route: '/products', icon: 'shopping_bag' },
  // { name: 'NAVBAR.CATEGORIES', route: '/categories', icon: 'category' },
  { name: 'NAVBAR.ABOUT', route: '/about', icon: 'info' },
  // {
  //   name: 'NAVBAR.ACCOUNT',
  //   icon: 'account_circle',
  //   subItems: [
  //     { name: 'NAVBAR.PROFILE', route: '/profile', icon: 'person' },
  //     { name: 'NAVBAR.ORDERS', route: '/orders', icon: 'list_alt' },
  //     { name: 'NAVBAR.LOGOUT', route: '/logout', icon: 'logout' }
  //   ]
  // },
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
  isSidebarOpen = false;
  cartItemCount = 0;
  navbarItems: MenuItem[] = NAVBAR_ITEMS;

  private translate = inject(TranslateService);
  public authService = inject(AuthService);
  private cartService = inject(CartService);

  isMobileView = false;

  constructor() {
    this.checkViewportSize();
    this.translate.addLangs(['en', 'vi']);
    this.translate.setDefaultLang('en');
  }
  ngOnInit() {
    // Alternatively, you can subscribe to the total quantity directly
    this.cartService.getTotalQuantity().subscribe(totalQuantity => {
      this.cartItemCount = totalQuantity;
    });
  }

  @HostListener('window:resize', ['$event'])
  onResize(): void {

    this.checkViewportSize();
  }

  private checkViewportSize(): void {
    this.isMobileView = window.innerWidth < 768; // Adjust the breakpoint as needed
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  switchLang(lang: string) {
    this.translate.use(lang);
  }

  onNavbarItemClick(item: MenuItem): void {
    if (item.name === 'NAVBAR.LOGOUT') {
      this.authService.logout();
    }
  }

  // Event handler for submenu items
  onNavbarSubItemClick(subItem: MenuItem): void {

  }

  onLogout() {
    this.authService.logout();
  }
}
