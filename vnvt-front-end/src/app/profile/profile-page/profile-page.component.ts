import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ChangePasswordDialogComponent } from '../change-password-dialog/change-password-dialog.component';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs'
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { CommonModule } from '@angular/common';
import { Order } from '../../core/models';
import { OrderService } from '../../core/services';
import { OrderDetailsDialogComponent } from '../order-details-dialog/order-details-dialog.component';
import { MatTableModule } from '@angular/material/table';
@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [MatButtonModule, MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatToolbarModule,
    MatIconModule, MatCardModule, CommonModule, MatTableModule, MatDialogModule],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss'
})
export class ProfilePageComponent {
  profileForm: FormGroup;
  profile: any = {};
  orders!: Order[];
  displayedColumns: string[] = ['id', 'totalAmount', 'paymentMethod', 'orderStatus', 'userId', 'actions'];

  user = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    country: '',
    zipcode: '',
    newsletter: false,
  };
  passwords = {
    current: '',
    new: '',
    confirm: '',
  };

  constructor(private fb: FormBuilder, private dialog: MatDialog,
    private http: HttpClient,
    private orderService: OrderService) {
    this.profileForm = this.fb.group({
      username: [{ value: 'User123', disabled: true }],
      email: [{ value: 'user@example.com', disabled: true }]
    });
  }

  ngOnInit(): void {
    this.orderService.getOrders().subscribe(x => this.orders = x)
    this.loadProfile();

  }
  onSaveChanges() {
    console.log('Profile saved', this.user);
    // Implement the save logic here
  }

  onEditPicture() {
    console.log('Edit picture clicked');
    // Implement the edit picture logic here
  }
  loadProfile(): void {
    const token = localStorage.getItem('auth_token');
    if (token) {
      this.http.get('https://your-api-url.com/profile', {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }).subscribe(data => {
        this.profile = data;
      });
    }
  }
  onChangePassword() {
    if (this.passwords.new !== this.passwords.confirm) {
      console.error('New password and confirm password do not match');
      return;
    }
    console.log('Password change', this.passwords);
    // Implement the password change logic here
  }
  openChangePasswordDialog(): void {
    const dialogRef = this.dialog.open(ChangePasswordDialogComponent, {
      width: '80%',
      height: '80%'
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result) {
        // Handle password change logic here
        console.log('Password changed:', result);
      }
    });
  }


  openOrderDetails(order: Order) {
    this.dialog.open(OrderDetailsDialogComponent, {
      width: '80vw',
      height: '80vh',
      data: order,
    });
  }
}
