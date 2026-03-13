import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { createTabStorage } from './helpers';

// ============ Price Alert Store ============
interface PriceAlertState {
    watchlist: string[]; // List of product codes
    toggleAlert: (productCode: string) => void;
    isWatched: (productCode: string) => boolean;
}

export const usePriceAlertStore = create<PriceAlertState>()(
    persist(
        (set, get) => ({
            watchlist: [],
            toggleAlert: (code) => {
                set((state) => ({
                    watchlist: state.watchlist.includes(code)
                        ? state.watchlist.filter((c) => c !== code)
                        : [...state.watchlist, code],
                }));
            },
            isWatched: (code) => get().watchlist.includes(code),
        }),
        {
            name: 'vnvt-price-alerts',
            storage: createJSONStorage(() => createTabStorage()),
        }
    )
);
