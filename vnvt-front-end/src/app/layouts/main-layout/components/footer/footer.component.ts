import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RECAPTCHA_SETTINGS, RecaptchaFormsModule, RecaptchaModule, RecaptchaSettings } from 'ng-recaptcha';
import { NgForm } from '@angular/forms';
@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatListModule,
    MatInputModule,
    MatButtonModule,
    MatToolbarModule,
    ReactiveFormsModule,
    FormsModule,
  ],
  providers: [
    {
      provide: RECAPTCHA_SETTINGS,
      useValue: {
        siteKey: '6LfMFhwqAAAAAGpVXRMYifLnW6q3Hjxh9hCxlqFr',
      } as RecaptchaSettings,
    },
  ],
})
export class FooterComponent {
  token: string | undefined;
  signupForm!: FormGroup;

  constructor(private fb: FormBuilder) {
    this.token = undefined;
  }


  public send(form: NgForm): void {
    if (form.invalid) {
      for (const control of Object.keys(form.controls)) {
        form.controls[control].markAsTouched();
      }
      return;
    }

    console.debug(`Token [${this.token}] generated`);
  }


  ngOnInit(): void {
    this.signupForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  getEmailErrorMessage() {
    if (this.signupForm.controls['email'].hasError('required')) {
      return 'You must enter an email';
    }
    return this.signupForm.controls['email'].hasError('email') ? 'Not a valid email' : '';
  }

  onSubmit(): void {
    if (this.signupForm.valid) {
      const email = this.signupForm.value.email;
      // Handle sign up logic here, e.g., send the email to your backend service
      console.log('Email submitted:', email);
      // Optionally, reset the form after submission
      this.signupForm.reset();
    }
  }
}
