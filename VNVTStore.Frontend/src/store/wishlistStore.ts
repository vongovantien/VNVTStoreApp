import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product } from '@/types';
import { createAuthStorage } from './helpers';

// ============ Wishlist Store ============
interface WishlistState {
    items: Product[];
    addItem: (product: Product) => void;
    removeItem: (productId: string) => void;
    isInWishlist: (productId: string) => boolean;
    clearWishlist: () => void;
}

export const useWishlistStore = create<WishlistState>()(
    persist(
        (set, get) => ({
            items: [],
            addItem: (product) => {
                set((state) => {
                    if (state.items.find((item) => item.code === product.code)) {
                        return state;
                    }
                    return { items: [...state.items, product] };
                });
            },
            removeItem: (productId) => {
                set((state) => ({
                    items: state.items.filter((item) => item.code !== productId),
                }));
            },
            isInWishlist: (productId) => {
                return get().items.some((item) => item.code === productId);
            },
            clearWishlist: () => set({ items: [] }),
        }),
        {
            name: 'vnvt-wishlist',
            storage: createJSONStorage(() => createAuthStorage()),
        }
    )
);
