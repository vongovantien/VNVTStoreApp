/**
 * Store barrel file.
 * Each store is defined in its own module for maintainability.
 * This file re-exports all stores so existing `import { ... } from '@/store'` continues to work.
 */

// Core stores
export { useCartStore } from './cartStore';
export { useAuthStore } from './authStore';
export { useWishlistStore } from './wishlistStore';
export { useCompareStore } from './compareStore';
export { useUIStore } from './uiStore';
export { useNotificationStore } from './notificationStore';
export { usePriceAlertStore } from './priceAlertStore';

// Feature stores
export { useToastStore, useToast } from './toastStore';
export type { Toast, ToastType } from './toastStore';
export * from './useSettings';
export * from './recentStore';
