import { create, StoreApi } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { User } from '@/types';
import { authService } from '@/services';
import { injectStore, AuthState } from '@/services/api';
import { useDiagnosticStore } from './diagnosticStore';
import { createAuthStorage } from './helpers';

// AuthState interface is imported from @/services/api to avoid circular dependency

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
                // Normalize permissions: extract code if object, otherwise use as is
                const normalizedPermissions = (user.permissions || []).map((p: string | { code: string }) =>
                    typeof p === 'object' && p !== null && 'code' in p ? p.code : String(p)
                );

                set({
                    user,
                    isAuthenticated: true,
                    token: token || null,
                    refreshToken: refreshToken || null,
                    permissions: normalizedPermissions,
                    menus: menus || []
                });

                useDiagnosticStore.getState().track({
                    module: 'AUTH',
                    eventType: 'LOGIN_SUCCESS',
                    description: `User ${user.email} logged in successfully`,
                    payload: { role: user.role, permissionsCount: normalizedPermissions.length },
                    severity: 'INFO'
                });

                // Note: Cart and Recent synchronization is now handled in store/effects.ts via subscription
                // to avoid circular dependencies.
            },
            logout: () => {
                const user = get().user;
                useDiagnosticStore.getState().track({
                    module: 'AUTH',
                    eventType: 'LOGOUT',
                    description: `User ${user?.email} logged out`,
                    payload: { email: user?.email },
                    severity: 'INFO'
                });
                set({ user: null, isAuthenticated: false, token: null, refreshToken: null, permissions: [], menus: [] });
            },
            updateUser: (userData: Partial<User>) => {
                const currentUser = get().user;
                if (currentUser) {
                    set({ user: { ...currentUser, ...userData } as User });
                }
            },
            impersonate: async (userCode: string) => {
                const currentToken = get().token;
                const isAdmin = String(get().user?.role).toUpperCase() === 'ADMIN';

                // Only allow if current user is an admin or we already have an adminToken stored
                if (!isAdmin && !get().adminToken) return;

                const res = await authService.impersonate(userCode);
                if (res.success && res.data) {
                    const { user, token, refreshToken } = res.data;

                    // Normalize permissions
                    const normalizedPermissions = (user.permissions || []).map((p: string | { code: string }) =>
                        typeof p === 'object' && p !== null && 'code' in p ? p.code : String(p)
                    );

                    set({
                        user: user as unknown as User,
                        token,
                        refreshToken: refreshToken || null,
                        adminToken: get().adminToken || currentToken, // Save admin token if not already saved
                        isAuthenticated: true,
                        permissions: normalizedPermissions,
                        menus: user.menus || []
                    });
                }
            },
            stopImpersonating: () => {
                const { adminToken } = get();
                if (adminToken) {
                    set({
                        user: null,
                        isAuthenticated: false,
                        token: null,
                        refreshToken: null,
                        adminToken: null,
                        permissions: [],
                        menus: []
                    });

                    // Redirect to login
                    window.location.href = '/login';
                }
            },
            setTokens: (token: string, refreshToken: string) => set({ token, refreshToken }),
            setPermissions: (permissions: string[]) => set({ permissions }),
            setMenus: (menus: string[]) => set({ menus }),
            hasPermission: (permission: string) => {
                const { permissions, user } = get();
                // Check if user is Admin (case-insensitive)
                if (String(user?.role).toUpperCase() === 'ADMIN') return true;
                return permissions.includes(permission);
            },
            hasMenu: (menuCode: string) => {
                const { menus, user } = get();
                if (String(user?.role).toUpperCase() === 'ADMIN') return true;
                return menus.includes(menuCode);
            },
        }),
        {
            name: 'vnvt-auth',
            storage: createJSONStorage(() => createAuthStorage()),
        }
    )
);

// Inject store to axios interceptor
injectStore(useAuthStore as unknown as StoreApi<AuthState>);
