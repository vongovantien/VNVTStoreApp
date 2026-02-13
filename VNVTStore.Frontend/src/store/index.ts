import { create, StoreApi } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { UserRole } from '@/types';
import type { Product, CartItem, User } from '@/types';
import { cartService, authService } from '@/services';
import { injectStore, AuthState } from '@/services/api';
import { useRecentStore } from './recentStore';

// ============ Helpers ============
/**
 * Get or generate a unique ID for the current browser tab.
 * Persists only as long as the tab/window is open.
 */
const getTabId = () => {
    if (typeof window === 'undefined') return 'server';
    let tabId = sessionStorage.getItem('vnvt-tab-id');
    if (!tabId) {
        tabId = Math.random().toString(36).substring(2, 9);
        sessionStorage.setItem('vnvt-tab-id', tabId);
    }
    return tabId;
};

const getStorageKey = (name: string) => {
    const tabId = getTabId();
    return `${name}-${tabId}`;
};

/**
 * Creates a tab-isolated storage wrapper.
 */
const createTabStorage = () => ({
    getItem: (name: string) => {
        const key = getStorageKey(name);
        return localStorage.getItem(key) || sessionStorage.getItem(key) || localStorage.getItem(name) || sessionStorage.getItem(name);
    },
    setItem: (name: string, value: string) => {
        const key = getStorageKey(name);
        const isRemember = localStorage.getItem('vnvt-remember') === 'true';
        if (isRemember) {
            localStorage.setItem(key, value);
            sessionStorage.removeItem(key);
        } else {
            sessionStorage.setItem(key, value);
            localStorage.removeItem(key);
        }
    },
    removeItem: (name: string) => {
        sessionStorage.removeItem(name);
        localStorage.removeItem(name);
    },
});

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

                        return {
                            items: [...state.items, {
                                code: newItemId,
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
                        productCode: product.code,
                        quantity,
                        size: options?.size,
                        color: options?.color
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

// ============ Auth Store ============
// AuthState interface is now imported from @/services/api to avoid circular dependency

const authStorage = {
    getItem: (name: string) => {
        const key = getStorageKey(name);
        return localStorage.getItem(key) || sessionStorage.getItem(key) || localStorage.getItem(name) || sessionStorage.getItem(name);
    },
    setItem: (name: string, value: string) => {
        const key = getStorageKey(name);
        const isRemember = localStorage.getItem('vnvt-remember') === 'true';
        if (isRemember) {
            localStorage.setItem(key, value);
            sessionStorage.removeItem(key);
        } else {
            sessionStorage.setItem(key, value);
            localStorage.removeItem(key);
        }
    },
    removeItem: (name: string) => {
        localStorage.removeItem(name);
        sessionStorage.removeItem(name);
    },
};

export const useAuthStore = create<AuthState>()(
    persist(
        (set, get) => ({
            user: null,
            isAuthenticated: false,
            token: null,
            refreshToken: null,
            adminToken: null,
            permissions: [],
            menus: [],
            login: async (user: User, token?: string, refreshToken?: string, menus?: string[]) => {
                set({
                    user,
                    isAuthenticated: true,
                    token: token || null,
                    refreshToken: refreshToken || null,
                    permissions: user.permissions || [],
                    menus: menus || []
                });

                // Sync local cart items to backend
                // Use optimistic sync - don't block login
                const { items, fetchCart } = useCartStore.getState();
                if (items.length > 0) {
                    Promise.all(items.map(item =>
                        cartService.addToCart({
                            productCode: item.product.code,
                            quantity: item.quantity,
                            size: item.size,
                            color: item.color
                        }).catch(() => console.error('[login] Failed to sync item', item))
                    )).then(() => fetchCart());
                } else {
                    await fetchCart();
                }

                // Merge recently viewed products
                const { viewedProducts, mergeRecent } = useRecentStore.getState();
                if (viewedProducts.length > 0) {
                    mergeRecent(viewedProducts);
                }
            },
            logout: () => set({ user: null, isAuthenticated: false, token: null, refreshToken: null }),
            updateUser: (userData: Partial<User>) => {
                const currentUser = get().user;
                if (currentUser) {
                    set({ user: { ...currentUser, ...userData } as User });
                }
            },
            impersonate: async (userCode: string) => {
                const currentToken = get().token;
                const isAdmin = get().user?.role === UserRole.Admin;

                // Only allow if current user is an admin or we already have an adminToken stored
                if (!isAdmin && !get().adminToken) return;

                const res = await authService.impersonate(userCode);
                if (res.success && res.data) {
                    const { user, token, refreshToken } = res.data;
                    set({
                        user: user as unknown as User,
                        token,
                        refreshToken: refreshToken || null,
                        adminToken: get().adminToken || currentToken, // Save admin token if not already saved
                        isAuthenticated: true,
                        permissions: user.permissions || [],
                        menus: user.menus || []
                    });
                }
            },
            stopImpersonating: () => {
                const { adminToken } = get();
                if (adminToken) {
                    // To fully recover admin state, we might need a "Back to Admin" API call 
                    // or just reload/re-fetch admin profile using the token.
                    // For now, simpler: clear impersonation and force a refresh or just logout.
                    // Actually, if we have the token, we can just logout and the user can re-login,
                    // OR we try to restore the admin session if we were smart about storing it.

                    // Simple approach: Logout everything to be safe
                    set({
                        user: null,
                        isAuthenticated: false,
                        token: null,
                        refreshToken: null,
                        adminToken: null,
                        permissions: [],
                        menus: []
                    });

                    // Optional: redirect to login
                    window.location.href = '/login';
                }
            },
            setTokens: (token: string, refreshToken: string) => set({ token, refreshToken }),
            setPermissions: (permissions: string[]) => set({ permissions }),
            setMenus: (menus: string[]) => set({ menus }),
            hasPermission: (permission: string) => {
                const { permissions, user } = get();
                // Admin role has all permissions usually, but we check the list
                if (user?.role === UserRole.Admin) return true;
                return permissions.includes(permission);
            },
            hasMenu: (menuCode: string) => {
                const { menus, user } = get();
                // Admin role has all menus
                if (user?.role === UserRole.Admin) return true;
                return menus.includes(menuCode);
            },
        }),
        {
            name: 'vnvt-auth',
            storage: createJSONStorage(() => authStorage),
        }
    )
);

// Inject store to axios interceptor
injectStore(useAuthStore as unknown as StoreApi<AuthState>);

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
            storage: createJSONStorage(() => localStorage),
        }
    )
);

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
            setQuickViewProduct: (product) => set({ quickViewProduct: product }),
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

// ============ Notification Store ============
interface NotificationState {
    notifications: string[];
    unreadCount: number;
    addNotification: (message: string) => void;
    markAllRead: () => void;
    clearNotifications: () => void;
}

export const useNotificationStore = create<NotificationState>()(
    persist(
        (set) => ({
            notifications: [],
            unreadCount: 0,
            addNotification: (message) => {
                set((state) => ({
                    notifications: [message, ...state.notifications],
                    unreadCount: state.unreadCount + 1,
                }));
            },
            markAllRead: () => set({ unreadCount: 0 }),
            clearNotifications: () => set({ notifications: [], unreadCount: 0 }),
        }),
        {
            name: 'vnvt-notifications',
            storage: createJSONStorage(() => localStorage),
        }
    )
);

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
            storage: createJSONStorage(() => localStorage),
        }
    )
);

// Re-export toast store
export { useToastStore, useToast } from './toastStore';
export type { Toast, ToastType } from './toastStore';
export * from './useSettings';
export * from './recentStore';
