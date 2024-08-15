import { Component, EventEmitter, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { BehaviorSubject } from 'rxjs';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { UserService } from '../../../../core/services';
import { AuthService } from '../../../../core/services/auth.service';
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [MatIconModule, MatToolbarModule, MatMenuModule, MatButtonModule, MatButtonToggleModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  @Output() toggle = new EventEmitter<void>();

  constructor(private userService: UserService, private authService: AuthService) { }

  toggleSidebar() {
    this.toggle.emit();
  }

  logout(): void {
    this.authService.logout();
    this.userService.clearUser();
  }
}
