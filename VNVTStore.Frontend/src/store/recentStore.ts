import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product } from '@/types';

interface RecentState {
    viewedProducts: Product[];
    addToRecent: (product: Product) => void;
    clearRecent: () => void;
    mergeRecent: (guestProducts: Product[]) => void;
}

const MAX_RECENT = 10;

export const useRecentStore = create<RecentState>()(
    persist(
        (set, get) => ({
            viewedProducts: [],

            addToRecent: (product) => {
                const { viewedProducts } = get();
                const filtered = viewedProducts.filter((p) => p.code !== product.code);
                const updated = [product, ...filtered].slice(0, MAX_RECENT);
                set({ viewedProducts: updated });
            },

            clearRecent: () => set({ viewedProducts: [] }),

            mergeRecent: (guestProducts) => {
                const { viewedProducts } = get();
                // Combine and deduplicate
                const combined = [...viewedProducts];

                guestProducts.forEach(guestProduct => {
                    const exists = combined.some(p => p.code === guestProduct.code);
                    if (!exists) {
                        combined.unshift(guestProduct);
                    }
                });

                set({ viewedProducts: combined.slice(0, MAX_RECENT) });
            },
        }),
        {
            name: 'vnvt-recent-viewed',
            storage: createJSONStorage(() => localStorage),
        }
    )
);
