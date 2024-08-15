import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-confirm',
  template: `
    <h1 mat-dialog-title>{{ data.title }}</h1>
    <div mat-dialog-content>
      <p>{{ data.message }}</p>
    </div>
    <div mat-dialog-actions>
      <button mat-raised-button color="primary" (click)="onConfirm()">OK</button>
      <button mat-raised-button color="accent" (click)="onCancel()">Cancel</button>
    </div>
  `,
  styles: [`
    mat-dialog-title {
      font-size: 1.2rem;
    }
    mat-dialog-content {
      font-size: 1rem;
    }
    mat-dialog-actions {
      justify-content: flex-end;
    }
  `],
  standalone: true,
  imports: [MatButtonModule, MatDialogModule]
})
export class AppConfirmComponent {
  constructor(
    public dialogRef: MatDialogRef<AppConfirmComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) { }

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}

interface ConfirmDialogData {
  title: string;
  message: string;
}
