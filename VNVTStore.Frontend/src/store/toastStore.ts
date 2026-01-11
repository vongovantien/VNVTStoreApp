import { create } from 'zustand';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: string;
  type: ToastType;
  message: string;
  duration?: number;
}

interface ToastStore {
  toasts: Toast[];
  addToast: (toast: Omit<Toast, 'id'>) => void;
  removeToast: (id: string) => void;
  clearAll: () => void;
  // Convenience methods
  success: (message: string, duration?: number) => void;
  error: (message: string, duration?: number) => void;
  warning: (message: string, duration?: number) => void;
  info: (message: string, duration?: number) => void;
}

let toastId = 0;

export const useToastStore = create<ToastStore>((set, get) => ({
  toasts: [],
  
  addToast: (toast) => {
    const id = `toast-${++toastId}`;
    const duration = toast.duration ?? 4000;
    
    set((state) => ({
      toasts: [...state.toasts, { ...toast, id }],
    }));
    
    // Auto remove after duration
    if (duration > 0) {
      setTimeout(() => {
        get().removeToast(id);
      }, duration);
    }
  },
  
  removeToast: (id) => {
    set((state) => ({
      toasts: state.toasts.filter((t) => t.id !== id),
    }));
  },
  
  clearAll: () => {
    set({ toasts: [] });
  },
  
  // Convenience methods
  success: (message, duration) => {
    get().addToast({ type: 'success', message, duration });
  },
  
  error: (message, duration) => {
    get().addToast({ type: 'error', message, duration });
  },
  
  warning: (message, duration) => {
    get().addToast({ type: 'warning', message, duration });
  },
  
  info: (message, duration) => {
    get().addToast({ type: 'info', message, duration });
  },
}));

// Export hook for easier access
export const useToast = () => {
  const { success, error, warning, info } = useToastStore();
  return { success, error, warning, info };
};
