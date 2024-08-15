import { Observable } from 'rxjs';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Injectable } from '@angular/core';

import { AppConfirmComponent } from './app-confirm.component';

@Injectable({
  providedIn: 'root'
})
export class AppConfirmService {

  constructor(private dialog: MatDialog) { }

  public confirm(data: ConfirmData = {}): Observable<boolean> {
    const dialogRef: MatDialogRef<AppConfirmComponent> = this.dialog.open(AppConfirmComponent, {
      width: '380px',
      disableClose: true,
      data: {
        title: data.title || 'Confirm',
        message: data.message || 'Are you sure?'
      }
    });

    return dialogRef.afterClosed();
  }
}

interface ConfirmData {
  title?: string;
  message?: string;
}
