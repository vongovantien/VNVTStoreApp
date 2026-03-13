import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product, CartItem } from '@/types';
import { cartService } from '@/services';
import { promotionService, type Promotion } from '@/services/promotionService';
import { useDiagnosticStore } from './diagnosticStore';
import { createTabStorage } from './helpers';

// NOTE: Circular dependency with authStore is safe here because
// useAuthStore is only accessed at runtime (inside functions),
// never during module initialization.
import { useAuthStore } from './authStore';

// ============ Cart Store ============
export interface CartState {
    items: CartItem[];
    isLoading: boolean;
    addItem: (product: Product, quantity?: number, options?: { size?: string; color?: string }) => Promise<void>;
    removeItem: (itemId: string) => Promise<void>;
    updateQuantity: (itemId: string, quantity: number) => Promise<void>;
    clearCart: () => Promise<void>;
    getTotal: () => number;
    getItemCount: () => number;
    fetchCart: () => Promise<void>;
    coupon: Promotion | null;
    discountAmount: number;
    applyCoupon: (code: string) => Promise<void>;
    removeCoupon: () => void;
}



export const useCartStore = create<CartState>()(
    persist(
        (set, get) => ({
            items: [],
            isLoading: false,
            coupon: null,
            discountAmount: 0,
            applyCoupon: async (code) => {
                set({ isLoading: true });
                try {
                    const res = await promotionService.getByCode(code);
                    if (res.success && res.data) {
                        const coupon = res.data;
                        useDiagnosticStore.getState().track({
                            module: 'CART',
                            eventType: 'COUPON_APPLY',
                            description: `Applied coupon ${code} with ${coupon.discountValue}${coupon.discountType === 'PERCENTAGE' ? '%' : 'đ'} discount`,
                            payload: { code, coupon },
                            severity: 'INFO'
                        });

                        // Check active date
                        const now = new Date();
                        if (new Date(coupon.startDate) > now || new Date(coupon.endDate) < now || !coupon.isActive) {
                            throw new Error('Mã giảm giá đã hết hạn hoặc chưa có hiệu lực');
                        }

                        // Validate minOrderAmount
                        const subtotal = get().getTotal();
                        if (coupon.minOrderAmount && subtotal < coupon.minOrderAmount) {
                            throw new Error(`Đơn hàng tối thiểu để áp dụng là ${coupon.minOrderAmount.toLocaleString('vi-VN')}đ`);
                        }

                        // Calculate discount
                        let discount = 0;
                        if (coupon.discountType === 'PERCENTAGE') {
                            discount = subtotal * (coupon.discountValue / 100);
                            if (coupon.maxDiscountAmount) {
                                discount = Math.min(discount, coupon.maxDiscountAmount);
                            }
                        } else {
                            discount = coupon.discountValue;
                        }

                        set({ coupon, discountAmount: discount });
                    } else {
                        throw new Error('Mã giảm giá không tồn tại');
                    }
                } catch (error) {
                    console.error('[applyCoupon] Failed', error);
                    throw error; // Re-throw for UI to catch
                } finally {
                    set({ isLoading: false });
                }
            },
            removeCoupon: () => set({ coupon: null, discountAmount: 0 }),
            fetchCart: async () => {
                const { isAuthenticated } = useAuthStore.getState();
                if (!isAuthenticated) return;

                set({ isLoading: true });
                try {
                    const res = await cartService.getMyCart();
                    if (res.success && res.data) {
                        set({ items: cartService.mapToFrontend(res.data) });
                    }
                } catch (error) {
                    console.error('[fetchCart] Failed to fetch cart', error);
                } finally {
                    set({ isLoading: false });
                }
            },
            addItem: async (product, quantity = 1, options) => {
                const { isAuthenticated } = useAuthStore.getState();

                // Optimistic Update for Guests
                if (!isAuthenticated) {
                    if (product.price <= 0) return;
                    set((state) => {
                        // Find item matching product ID AND options
                        const existingItem = state.items.find((item) =>
                            item.product.code === product.code &&
                            item.size === options?.size &&
                            item.color === options?.color
                        );

                        if (existingItem) {
                            return {
                                items: state.items.map((item) =>
                                    item.code === existingItem.code
                                        ? { ...item, quantity: item.quantity + quantity }
                                        : item
                                ),
                            };
                        }

                        // Create unique ID for guest item
                        const newItemId = `guest_${product.code}_${options?.size || ''}_${options?.color || ''}_${Date.now()}`;

                        useDiagnosticStore.getState().track({
                            module: 'CART',
                            eventType: 'ITEM_ADD_GUEST',
                            description: `Guest added ${quantity}x ${product.name} to cart`,
                            payload: { productCode: product.code, quantity, options },
                            severity: 'INFO'
                        });

                        return {
                            items: [...state.items, {
                                code: newItemId,
                                product,
                                quantity,
                                ...(options?.size && { size: options.size }),
                                ...(options?.color && { color: options.color })
                            } as CartItem],
                        };
                    });
                    return;
                }

                // API Call for Users
                set({ isLoading: true });
                try {
                    const res = await cartService.addToCart({
                        productCode: product.code,
                        quantity,
                        ...(options?.size && { size: options.size }),
                        ...(options?.color && { color: options.color })
                    });
                    if (!res.success) {
                        throw new Error(res.message || 'Add to cart failed');
                    }

                    if (res.success && res.data) {
                        set({ items: cartService.mapToFrontend(res.data) });
                    }
                } catch (error) {
                    console.error('[addItem] Add to cart failed', error);
                    throw error;
                } finally {
                    set({ isLoading: false });
                }
            },
            removeItem: async (itemId) => {
                const { isAuthenticated } = useAuthStore.getState();
                if (!isAuthenticated) {
                    set((state) => ({
                        items: state.items.filter((item) => item.code !== itemId),
                    }));
                    return;
                }

                try {
                    const res = await cartService.removeFromCart(itemId);
                    if (res.success && res.data) {
                        useDiagnosticStore.getState().track({
                            module: 'CART',
                            eventType: 'ITEM_REMOVE',
                            description: `Removed item ${itemId} from cart`,
                            payload: { itemId },
                            severity: 'INFO'
                        });
                        set({ items: cartService.mapToFrontend(res.data) });
                    }
                } finally {
                    set({ isLoading: false });
                }
            },
            updateQuantity: async (itemId, quantity) => {
                if (quantity <= 0) {
                    get().removeItem(itemId);
                    return;
                }

                const { isAuthenticated } = useAuthStore.getState();
                if (!isAuthenticated) {
                    set((state) => ({
                        items: state.items.map((item) =>
                            item.code === itemId ? { ...item, quantity } : item
                        ),
                    }));
                    return;
                }

                try {
                    const res = await cartService.updateCartItem({ itemCode: itemId, quantity });
                    if (!res.success) {
                        throw new Error(res.message || 'Update quantity failed');
                    }

                    if (res.success && res.data) {
                        useDiagnosticStore.getState().track({
                            module: 'CART',
                            eventType: 'QUANTITY_UPDATE',
                            description: `Updated item ${itemId} quantity to ${quantity}`,
                            payload: { itemId, quantity },
                            severity: 'INFO'
                        });
                        set({ items: cartService.mapToFrontend(res.data) });
                    }
                } catch (error) {
                    console.error('[updateQuantity] Update quantity failed', error);
                }
            },
            clearCart: async () => {
                const { isAuthenticated } = useAuthStore.getState();
                if (isAuthenticated) {
                    await cartService.clearCart();
                }
                useDiagnosticStore.getState().track({
                    module: 'CART',
                    eventType: 'CART_CLEAR',
                    description: 'Cart cleared successfully',
                    payload: { wasAuthenticated: isAuthenticated },
                    severity: 'INFO'
                });
                set({ items: [] });
            },
            getTotal: () => {
                return get().items.reduce(
                    (total, item) => total + item.product.price * item.quantity,
                    0
                );
            },
            getItemCount: () => {
                return get().items.reduce((count, item) => count + item.quantity, 0);
            },
        }),
        {
            name: 'vnvt-cart',
            storage: createJSONStorage(() => createTabStorage()),
            partialize: (state) => ({ items: state.items }), // Don't persist isLoading
        }
    )
);
