import { Component, Signal, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormValidators } from '../../validators/form.validator';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  registerForm!: FormGroup;
  submitted = false;
  get f() {
    return this.registerForm.controls;
  }
  constructor(private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router) {
    this.initForm();
  }
  hidePassword = signal(true);
  hideConfirmPassword = signal(true);
  initForm() {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, {
      validator: FormValidators.mustMatch('password', 'confirmPassword')
    });
  }
  toggleVisibility(field: 'password' | 'confirmPassword') {
    if (field === 'password') {
      this.hidePassword.update(h => !h);
    } else if (field === 'confirmPassword') {
      this.hideConfirmPassword.update(h => !h);
    }
  }

  onSubmit() {
    this.submitted = true;
    if (this.registerForm.invalid) {
      return
    }
    this.authService.register(this.registerForm.value).subscribe(
      response => {
        // Handle successful registration
        this.toastService.showToast(response.message, 'success');
        this.router.navigate(['/login']);

      },
      // error => {
      //   console.log(error)
      //   // Handle registration error
      //   this.toastService.showToast(error.error.Message, 'error');
      // }
    );
  }
}
