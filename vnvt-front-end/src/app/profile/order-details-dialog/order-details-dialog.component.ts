import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { Order } from '../../core/models';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
@Component({
  selector: 'app-order-details-dialog',
  standalone: true,
  imports: [
    MatTableModule,
    MatDialogModule,
    CommonModule,
    MatButtonModule,
    MatFormFieldModule
  ],
  templateUrl: './order-details-dialog.component.html',
  styleUrl: './order-details-dialog.component.scss'
})
export class OrderDetailsDialogComponent {
  displayedColumns: string[] = ['productId', 'quantity', 'price', 'total'];

  constructor(@Inject(MAT_DIALOG_DATA) public order: Order) { }

  ngOnInit(): void {
    console.log(this.order)
  }

  getTotalCost(): number {
    console.log(this.order.items)
    return this.order.items.reduce((acc: any, item: any) => acc + item.quantity * item.price, 0);
  }
}
