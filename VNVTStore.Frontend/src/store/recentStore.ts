import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product } from '@/types';
import { createTabStorage } from './helpers';

interface RecentState {
    viewedProducts: Product[];
    searchHistory: string[];
    addToRecent: (product: Product) => void;
    addSearchQuery: (query: string) => void;
    clearRecent: () => void;
    clearSearchHistory: () => void;
    mergeRecent: (guestProducts: Product[]) => void;
}

const MAX_RECENT = 10;

export const useRecentStore = create<RecentState>()(
    persist(
        (set, get) => ({
            viewedProducts: [],
            searchHistory: [],

            addToRecent: (product) => {
                const { viewedProducts } = get();
                const filtered = viewedProducts.filter((p) => p.code !== product.code);
                const updated = [product, ...filtered].slice(0, MAX_RECENT);
                set({ viewedProducts: updated });
            },

            addSearchQuery: (query) => {
                if (!query.trim()) return;
                const { searchHistory } = get();
                const filtered = searchHistory.filter((q) => q.toLowerCase() !== query.toLowerCase());
                const updated = [query, ...filtered].slice(0, 5); // Keep last 5 searches
                set({ searchHistory: updated });
            },

            clearRecent: () => set({ viewedProducts: [] }),
            clearSearchHistory: () => set({ searchHistory: [] }),

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
            storage: createJSONStorage(() => createTabStorage()),
        }
    )
);
