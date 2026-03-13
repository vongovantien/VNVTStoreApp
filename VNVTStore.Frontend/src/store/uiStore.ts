import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product } from '@/types';
import { createTabStorage } from './helpers';

// ============ UI Store ============
interface UIState {
    theme: 'light' | 'dark' | 'cyberpunk';
    sidebarOpen: boolean;
    searchOpen: boolean;
    cartOpen: boolean;
    quickViewProduct: Product | null;
    toggleTheme: () => void;
    setSidebarOpen: (open: boolean) => void;
    setSearchOpen: (open: boolean) => void;
    setCartOpen: (open: boolean) => void;
    setQuickViewProduct: (product: Product | null) => void;
}

export const useUIStore = create<UIState>()(
    persist(
        (set, get) => ({
            theme: 'light',
            sidebarOpen: true,
            searchOpen: false,
            cartOpen: false,
            quickViewProduct: null,
            toggleTheme: () => {
                const themes: UIState['theme'][] = ['light', 'dark', 'cyberpunk'];
                const currentIndex = themes.indexOf(get().theme);
                const nextIndex = (currentIndex + 1) % themes.length;
                const newTheme = themes[nextIndex];

                document.documentElement.setAttribute('data-theme', newTheme);
                if (newTheme !== 'light') {
                    document.documentElement.classList.add('dark');
                } else {
                    document.documentElement.classList.remove('dark');
                }
                set({ theme: newTheme });
            },
            setSidebarOpen: (open) => set({ sidebarOpen: open }),
            setSearchOpen: (open) => set({ searchOpen: open }),
            setCartOpen: (open) => set({ cartOpen: open }),
            setQuickViewProduct: (product) => set({ quickViewProduct: product }),
        }),
        {
            name: 'vnvt-ui',
            storage: createJSONStorage(() => createTabStorage()),
            partialize: (state) => ({ theme: state.theme, sidebarOpen: state.sidebarOpen }),
        }
    )
);

// Initialize theme on load
if (typeof window !== 'undefined' && window.localStorage) {
    const stored = localStorage.getItem('vnvt-ui');
    if (stored) {
        try {
            const { state } = JSON.parse(stored);
            if (state?.theme) {
                document.documentElement.setAttribute('data-theme', state.theme);
                if (state.theme === 'dark') {
                    document.documentElement.classList.add('dark');
                }
            }
        } catch {
            // Ignore parse errors
        }
    }
}
