import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
  duration: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastSubject = new BehaviorSubject<Toast[]>([]);
  toasts$ = this.toastSubject.asObservable();

  showToast(message: string, type: 'success' | 'error' | 'info' | 'warning', duration = 3000) {
    const toast: Toast = { message, type, duration };
    this.toastSubject.next([...this.toastSubject.getValue(), toast]);
    setTimeout(() => this.removeToast(toast), duration);
  }

  private removeToast(toast: Toast) {
    this.toastSubject.next(this.toastSubject.getValue().filter(t => t !== toast));
  }
}
