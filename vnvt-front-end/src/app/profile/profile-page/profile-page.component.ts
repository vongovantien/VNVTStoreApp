import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ControlValueAccessor, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Observable } from 'rxjs';
import { Order, User } from '../../core/models';
import { ApiResponse } from '../../core/models/api-response.model';
import { OrderService, ToastService, UserService } from '../../core/services';
import { ChangePasswordDialogComponent } from '../change-password-dialog/change-password-dialog.component';
import { ImageCropperComponent } from '../image-cropper/image-cropper.component';
import { OrderDetailsDialogComponent } from '../order-details-dialog/order-details-dialog.component';
@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [MatButtonModule, MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatToolbarModule,
    MatIconModule,
    MatCardModule,
    CommonModule,
    MatTableModule,
    MatDialogModule,
    ReactiveFormsModule],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss'
})
export class ProfilePageComponent implements OnInit, ControlValueAccessor {
  profileForm: FormGroup;
  passwordForm: FormGroup;

  profile: unknown = {};
  orders!: Order[];
  displayedColumns: string[] = ['id', 'totalAmount', 'paymentMethod', 'orderStatus', 'userId', 'actions'];

  user!: User;
  passwords = {
    current: '',
    new: '',
    confirm: '',
  };
  constructor(private fb: FormBuilder, private dialog: MatDialog,
    private http: HttpClient,
    private orderService: OrderService, private userService: UserService, private toastService: ToastService) {
    this.profileForm = this.fb.group({
      firstname: [null],
      lastname: [null],
      email: [null, [Validators.required, Validators.email]],
      phone: [null],
      address: [null],
      city: [null],
      country: [null],
      zipcode: [null]
    });

    this.passwordForm = this.fb.group({
      currentPassword: [null, Validators.required],
      newPassword: [null, [Validators.required, Validators.minLength(6)]],
      confirmPassword: [null, Validators.required]
    });
  }

  loadUserData(userId: number): void {
    this.userService.getUserById(userId).subscribe(
      (userData: User) => {
        this.user = userData;
      },
      (error) => {
        console.error('Error loading user data', error);
      }
    );
  }
  onChange = (fileUrl: string) => { console.log(fileUrl) };

  onTouched = () => { };

  disabled: boolean = false;

  writeValue(_file: string): void {
    this.file = _file;
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }
  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  ngOnInit(): void {
    this.orderService.getOrders().subscribe(x => this.orders = x)
    this.loadProfile();

  }
  onSaveChanges() {
    const userData: User = {
      ...this.profileForm.value
    };
    console.log(userData)
    this.userService.updateProfile(userData).subscribe(
      (res: ApiResponse<any>) => {
        if (res.success) {
          this.toastService.showToast('Profile updated successfully', 'success');
        }
      },
      (error) => {
        console.error('Error loading user data', error);
      }
    );
  }

  onEditPicture() {
    console.log('Edit picture clicked');
    // Implement the edit picture logic here
  }

  loadProfile(): void {
    this.userService.getUserProfile().subscribe({
      next: (res: any) => {
        console.log(res.data)
        this.profile = res.data;
        this.patchValueForm(this.profile)
      },
      error: (err) => {
        console.error('Error loading profile:', err);
      },
    });
  }
  patchValueForm(data: any) {
    this.profileForm.patchValue({
      firstname: data.firstname,
      lastname: data.lastname,
      email: data.email,
      phone: data.phone,
      address: data.address,
      city: data.city,
      country: data.country,
      zipcode: data.zipcode,
    });
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

  onFileChange(event: any) {
    const files = event.target.files as FileList;

    if (files.length > 0) {
      const _file = URL.createObjectURL(files[0]);
      this.resetInput();
      this.openAvatarEditor(_file)
        .subscribe(
          (result) => {
            if (result) {
              console.log(result)
              this.file = result;
              this.onChange(this.file); // <= HERE
            }
          }
        )
    }
  }

  resetInput() {
    const input = document.getElementById('avatar-input-file') as HTMLInputElement;
    if (input) {
      input.value = "";
    }
  }

  openAvatarEditor(image: string): Observable<any> {
    const dialogRef = this.dialog.open(ImageCropperComponent, {
      maxWidth: '80vw',
      maxHeight: '80vh',
      data: image,
    });

    return dialogRef.afterClosed();
  }
}
