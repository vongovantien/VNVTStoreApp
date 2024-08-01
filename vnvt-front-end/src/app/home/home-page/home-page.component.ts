import { Component } from '@angular/core';
import { ProductService } from '../../core/services';
import { Product } from '../../core/models';
import { CommonModule } from '@angular/common';
import { CardProductComponent } from '../../card-product/card-product.component';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RECAPTCHA_SETTINGS, RecaptchaFormsModule, RecaptchaModule, RecaptchaSettings } from 'ng-recaptcha';
import { NgForm } from '@angular/forms';
@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, CardProductComponent, MatIconModule, MatFormFieldModule, MatInputModule, RecaptchaModule,
    RecaptchaFormsModule],
  providers: [
    {
      provide: RECAPTCHA_SETTINGS,
      useValue: {
        siteKey: '6LfMFhwqAAAAAGpVXRMYifLnW6q3Hjxh9hCxlqFr',
      } as RecaptchaSettings,
    },
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {
  products: Product[] = [];
  token: string | undefined;
  constructor(private productService: ProductService) {
    this.token = undefined;
  }

  ngOnInit(): void {
    this.productService.getProducts().subscribe((data: Product[]) => {
      console.log("test")
      this.products = data;
    });
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


  onRemoveItem(item: any) { }
}
