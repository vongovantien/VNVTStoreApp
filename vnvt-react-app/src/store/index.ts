import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product, CartItem, User } from '@/types';

// ============ Cart Store ============
interface CartState {
    items: CartItem[];
    addItem: (product: Product, quantity?: number) => void;
    removeItem: (productId: string) => void;
    updateQuantity: (productId: string, quantity: number) => void;
    clearCart: () => void;
    getTotal: () => number;
    getItemCount: () => number;
}

export const useCartStore = create<CartState>()(
    persist(
        (set, get) => ({
            items: [],
            addItem: (product, quantity = 1) => {
                // Only add products with fixed price
                if (product.price <= 0) return;

                set((state) => {
                    const existingItem = state.items.find((item) => item.product.id === product.id);
                    if (existingItem) {
                        return {
                            items: state.items.map((item) =>
                                item.product.id === product.id
                                    ? { ...item, quantity: item.quantity + quantity }
                                    : item
                            ),
                        };
                    }
                    return {
                        items: [...state.items, { id: product.id, product, quantity }],
                    };
                });
            },
            removeItem: (productId) => {
                set((state) => ({
                    items: state.items.filter((item) => item.product.id !== productId),
                }));
            },
            updateQuantity: (productId, quantity) => {
                if (quantity <= 0) {
                    get().removeItem(productId);
                    return;
                }
                set((state) => ({
                    items: state.items.map((item) =>
                        item.product.id === productId ? { ...item, quantity } : item
                    ),
                }));
            },
            clearCart: () => set({ items: [] }),
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
            storage: createJSONStorage(() => localStorage),
        }
    )
);

// ============ Auth Store ============
interface AuthState {
    user: User | null;
    isAuthenticated: boolean;
    token: string | null;
    login: (user: User, token?: string) => void;
    logout: () => void;
    updateUser: (userData: Partial<User>) => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set, get) => ({
            user: null,
            isAuthenticated: false,
            token: null,
            login: (user, token) => set({ user, isAuthenticated: true, token: token || null }),
            logout: () => set({ user: null, isAuthenticated: false, token: null }),
            updateUser: (userData) => {
                const currentUser = get().user;
                if (currentUser) {
                    set({ user: { ...currentUser, ...userData } });
                }
            },
        }),
        {
            name: 'vnvt-auth',
            storage: createJSONStorage(() => localStorage),
        }
    )
);

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
                    if (state.items.find((item) => item.id === product.id)) {
                        return state;
                    }
                    return { items: [...state.items, product] };
                });
            },
            removeItem: (productId) => {
                set((state) => ({
                    items: state.items.filter((item) => item.id !== productId),
                }));
            },
            isInWishlist: (productId) => {
                return get().items.some((item) => item.id === productId);
            },
            clearWishlist: () => set({ items: [] }),
        }),
        {
            name: 'vnvt-wishlist',
            storage: createJSONStorage(() => localStorage),
        }
    )
);

// ============ Compare Store ============
interface CompareState {
    items: Product[];
    maxItems: number;
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
            addItem: (product) => {
                set((state) => {
                    if (state.items.length >= state.maxItems) {
                        // Remove first item and add new one
                        const newItems = [...state.items.slice(1), product];
                        return { items: newItems };
                    }
                    if (state.items.find((item) => item.id === product.id)) {
                        return state;
                    }
                    return { items: [...state.items, product] };
                });
            },
            removeItem: (productId) => {
                set((state) => ({
                    items: state.items.filter((item) => item.id !== productId),
                }));
            },
            isInCompare: (productId) => {
                return get().items.some((item) => item.id === productId);
            },
            clearCompare: () => set({ items: [] }),
        }),
        {
            name: 'vnvt-compare',
            storage: createJSONStorage(() => localStorage),
        }
    )
);

// ============ UI Store ============
interface UIState {
    theme: 'light' | 'dark';
    sidebarOpen: boolean;
    searchOpen: boolean;
    cartOpen: boolean;
    toggleTheme: () => void;
    setSidebarOpen: (open: boolean) => void;
    setSearchOpen: (open: boolean) => void;
    setCartOpen: (open: boolean) => void;
}

export const useUIStore = create<UIState>()(
    persist(
        (set, get) => ({
            theme: 'light',
            sidebarOpen: true,
            searchOpen: false,
            cartOpen: false,
            toggleTheme: () => {
                const newTheme = get().theme === 'light' ? 'dark' : 'light';
                document.documentElement.setAttribute('data-theme', newTheme);
                set({ theme: newTheme });
            },
            setSidebarOpen: (open) => set({ sidebarOpen: open }),
            setSearchOpen: (open) => set({ searchOpen: open }),
            setCartOpen: (open) => set({ cartOpen: open }),
        }),
        {
            name: 'vnvt-ui',
            storage: createJSONStorage(() => localStorage),
            partialize: (state) => ({ theme: state.theme, sidebarOpen: state.sidebarOpen }),
        }
    )
);

// Initialize theme on load
if (typeof window !== 'undefined') {
    const stored = localStorage.getItem('vnvt-ui');
    if (stored) {
        try {
            const { state } = JSON.parse(stored);
            if (state?.theme) {
                document.documentElement.setAttribute('data-theme', state.theme);
            }
        } catch {
            // Ignore parse errors
        }
    }
}

// Re-export toast store
export { useToastStore, useToast } from './toastStore';
export type { Toast, ToastType } from './toastStore';
