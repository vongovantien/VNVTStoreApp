import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product } from '@/types';
import { createTabStorage } from './helpers';

// ============ Compare Store ============
interface CompareState {
    items: Product[];
    maxItems: number;
    isOpen: boolean;
    setIsOpen: (open: boolean) => void;
    addItem: (product: Product) => void;
    removeItem: (productId: string) => void;
    isInCompare: (productId: string) => boolean;
    clearCompare: () => void;
}

export const useCompareStore = create<CompareState>()(
    persist(
        (set, get) => ({
            items: [],
            maxItems: 3,
            isOpen: false,
            setIsOpen: (open) => set({ isOpen: open }),
            addItem: (product) => {
                set((state) => {
                    if (state.items.length >= state.maxItems) {
                        // Remove first item and add new one
                        const newItems = [...state.items.slice(1), product];
                        return { items: newItems };
                    }
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
            isInCompare: (productId) => {
                return get().items.some((item) => item.code === productId);
            },
            clearCompare: () => set({ items: [] }),
        }),
        {
            name: 'vnvt-compare',
            storage: createJSONStorage(() => createTabStorage()),
        }
    )
);
