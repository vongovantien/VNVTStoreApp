import { CommonModule } from '@angular/common';
import { Component, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { Product } from '../../core/models';
import { ProductService } from '../../core/services';
import { ProductCreateComponent } from './components/product-create/product-create.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [RouterModule, CommonModule, MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatButtonModule,],
  templateUrl: './admin-products.component.html',
  styleUrl: './admin-products.component.scss'
})
export class AdminProductsComponent {
  displayedColumns: string[] = ['id', 'name', 'description', 'price', 'stock', 'actions'];
  dataSource: MatTableDataSource<Product>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private productService: ProductService, public dialog: MatDialog) {
    this.dataSource = new MatTableDataSource();
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(ProductCreateComponent, {
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Thêm sản phẩm mới sau khi đóng dialog
        this.createProduct(result);
      }
    });
  }

  createProduct(product: Product): void {
    this.productService.createProduct(product).subscribe(() => {
      this.loadProducts();
    });
  }


  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe((data: Product[]) => {
      this.dataSource.data = data;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  deleteProduct(id: number): void {
    this.productService.deleteProduct(id).subscribe(() => {
      this.loadProducts();
    });
  }

  importData(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const formData = new FormData();
      formData.append('file', file);
      // this.productService.importData(formData).subscribe(() => {
      //   this.loadProducts();
      // });
    }
  }

  exportData(): void {
    // this.productService.exportData().subscribe((data: Blob) => {
    //   const a = document.createElement('a');
    //   const objectUrl = URL.createObjectURL(data);
    //   a.href = objectUrl;
    //   a.download = 'products.xlsx';
    //   a.click();
    //   URL.revokeObjectURL(objectUrl);
    // });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}
