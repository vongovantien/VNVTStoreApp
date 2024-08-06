import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { CardProductComponent } from '../../card-product/card-product.component';
import { Product } from '../../core/models';
import { ProductService } from '../../core/services';
@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, CardProductComponent, MatIconModule, MatFormFieldModule, MatInputModule],
  providers: [],
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
