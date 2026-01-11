import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { Product, CartItem, User } from '@/types';

// ============ Cart Store ============
interface CartState {
    items: CartItem[];
    isLoading: boolean;
    addItem: (product: Product, quantity?: number, options?: { size?: string; color?: string }) => Promise<void>;
    removeItem: (itemId: string) => Promise<void>;
    updateQuantity: (itemId: string, quantity: number) => Promise<void>;
    clearCart: () => Promise<void>;
    getTotal: () => number;
    getItemCount: () => number;
    fetchCart: () => Promise<void>;
}

import { cartService } from '@/services';

export const useCartStore = create<CartState>()(
    persist(
        (set, get) => ({
            items: [],
            isLoading: false,
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
                    console.error('Failed to fetch cart', error);
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
                            item.product.id === product.id &&
                            item.size === options?.size &&
                            item.color === options?.color
                        );

                        if (existingItem) {
                            return {
                                items: state.items.map((item) =>
                                    item.id === existingItem.id
                                        ? { ...item, quantity: item.quantity + quantity }
                                        : item
                                ),
                            };
                        }

                        // Create unique ID for guest item
                        const newItemId = `guest_${product.id}_${options?.size || ''}_${options?.color || ''}_${Date.now()}`;

                        return {
                            items: [...state.items, {
                                id: newItemId,
                                product,
                                quantity,
                                size: options?.size,
                                color: options?.color
                            }],
                        };
                    });
                    return;
                }

                // API Call for Users
                set({ isLoading: true });
                try {
                    const res = await cartService.addToCart({
                        productCode: product.id,
                        quantity,
                        size: options?.size,
                        color: options?.color
                    });
                    if (res.success && res.data) {
                        set({ items: cartService.mapToFrontend(res.data) });
                    }
                } catch (error) {
                    console.error('Add to cart failed', error);
                } finally {
                    set({ isLoading: false });
                }
            },
            removeItem: async (itemId) => {
                const { isAuthenticated } = useAuthStore.getState();
                if (!isAuthenticated) {
                    set((state) => ({
                        items: state.items.filter((item) => item.id !== itemId),
                    }));
                    return;
                }

                set({ isLoading: true });
                try {
                    const res = await cartService.removeFromCart(itemId);
                    if (res.success && res.data) {
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
                            item.id === itemId ? { ...item, quantity } : item
                        ),
                    }));
                    return;
                }

                try {
                    const res = await cartService.updateCartItem({ itemCode: itemId, quantity });
                    if (res.success && res.data) {
                        set({ items: cartService.mapToFrontend(res.data) });
                    }
                } catch (error) {
                    console.error('Update quantity failed', error);
                }
            },
            clearCart: async () => {
                const { isAuthenticated } = useAuthStore.getState();
                if (isAuthenticated) {
                    await cartService.clearCart();
                }
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
            storage: createJSONStorage(() => localStorage),
            partialize: (state) => ({ items: state.items }), // Don't persist isLoading
        }
    )
);

// ============ Auth Store ============
interface AuthState {
    user: User | null;
    isAuthenticated: boolean;
    token: string | null;
    refreshToken: string | null;
    login: (user: User, token?: string, refreshToken?: string) => Promise<void>;
    logout: () => void;
    updateUser: (userData: Partial<User>) => void;
    setTokens: (token: string, refreshToken: string) => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set, get) => ({
            user: null,
            isAuthenticated: false,
            token: null,
            refreshToken: null,
            login: async (user, token, refreshToken) => {
                set({ user, isAuthenticated: true, token: token || null, refreshToken: refreshToken || null });

                // Sync local cart items to backend
                const { items, fetchCart } = useCartStore.getState();
                if (items.length > 0) {
                    for (const item of items) {
                        try {
                            await cartService.addToCart({
                                productCode: item.product.id,
                                quantity: item.quantity,
                                size: item.size,
                                color: item.color
                            });
                        } catch (err) {
                            console.error('Failed to sync item', item);
                        }
                    }
                }
                // Fetch latest cart from backend (merges backend state)
                await fetchCart();
            },
            logout: () => set({ user: null, isAuthenticated: false, token: null, refreshToken: null }),
            updateUser: (userData) => {
                const currentUser = get().user;
                if (currentUser) {
                    set({ user: { ...currentUser, ...userData } });
                }
            },
            setTokens: (token, refreshToken) => set({ token, refreshToken }),
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
                if (newTheme === 'dark') {
                    document.documentElement.classList.add('dark');
                } else {
                    document.documentElement.classList.remove('dark');
                }
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
                if (state.theme === 'dark') {
                    document.documentElement.classList.add('dark');
                }
            }
        } catch {
            // Ignore parse errors
        }
    }
}

// Re-export toast store
export { useToastStore, useToast } from './toastStore';
export type { Toast, ToastType } from './toastStore';
