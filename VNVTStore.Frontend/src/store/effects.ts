import { useAuthStore } from './authStore';
import { useCartStore } from './cartStore';
import { useRecentStore } from './recentStore';
import { cartService } from '@/services';

export const initializeStoreEffects = () => {
    // Auth -> Cart/Recent Sync
    // Subscribe to auth state changes to trigger sync on login
    useAuthStore.subscribe((state, prevState) => {
        // Check if user just logged in
        if (state.isAuthenticated && !prevState.isAuthenticated) {
            console.log('[AuthEffects] User logged in, syncing cart and recent products...');

            // 1. Sync Cart
            const { items, fetchCart } = useCartStore.getState();
            if (items.length > 0) {
                Promise.all(items.map(item =>
                    cartService.addToCart({
                        productCode: item.product.code,
                        quantity: item.quantity,
                        ...(item.size && { size: item.size }),
                        ...(item.color && { color: item.color })
                    }).catch(err => console.error('[AuthEffects] Failed to sync item', item, err))
                )).then(() => {
                    fetchCart(); // Fetch server cart after sync
                });
            } else {
                fetchCart(); // Just fetch server cart
            }

            // 2. Sync Recent Products
            const { viewedProducts, mergeRecent } = useRecentStore.getState();
            if (viewedProducts.length > 0) {
                mergeRecent(viewedProducts);
            }
        }
    });
};
