/**
 * Shared storage helpers for tab-isolated Zustand persistence.
 */

/**
 * Get or generate a unique ID for the current browser tab.
 * Persists only as long as the tab/window is open.
 */
export const getTabId = () => {
    if (typeof window === 'undefined' || !window.sessionStorage) return 'server';
    try {
        let tabId = sessionStorage.getItem('vnvt-tab-id');
        if (!tabId) {
            tabId = Math.random().toString(36).substring(2, 9);
            sessionStorage.setItem('vnvt-tab-id', tabId);
        }
        return tabId;
    } catch (e) {
        console.warn('Session storage access denied', e);
        return 'temp-tab';
    }
};

export const getStorageKey = (name: string) => {
    const tabId = getTabId();
    return `${name}-${tabId}`;
};

/**
 * Creates a tab-isolated storage wrapper.
 * Uses localStorage if "remember me" is enabled, otherwise sessionStorage.
 */
export const createTabStorage = () => ({
    getItem: (name: string) => {
        const key = getStorageKey(name);
        try {
            return localStorage.getItem(key) || sessionStorage.getItem(key) || localStorage.getItem(name) || sessionStorage.getItem(name);
        } catch {
            return null;
        }
    },
    setItem: (name: string, value: string) => {
        const key = getStorageKey(name);
        try {
            const isRemember = localStorage.getItem('vnvt-remember') === 'true';
            if (isRemember) {
                localStorage.setItem(key, value);
                sessionStorage.removeItem(key);
            } else {
                sessionStorage.setItem(key, value);
                localStorage.removeItem(key);
            }
        } catch (e) {
            console.warn('Storage set failed', e);
        }
    },
    removeItem: (name: string) => {
        try {
            sessionStorage.removeItem(name);
            localStorage.removeItem(name);
        } catch { /* empty */ }
    },
});

/**
 * Auth-specific storage (same logic as tab storage but separate for clarity).
 */
export const createAuthStorage = () => ({
    getItem: (name: string) => {
        const key = getStorageKey(name);
        try {
            return localStorage.getItem(key) || sessionStorage.getItem(key) || localStorage.getItem(name) || sessionStorage.getItem(name);
        } catch {
            return null;
        }
    },
    setItem: (name: string, value: string) => {
        const key = getStorageKey(name);
        try {
            const isRemember = localStorage.getItem('vnvt-remember') === 'true';
            if (isRemember) {
                localStorage.setItem(key, value);
                sessionStorage.removeItem(key);
            } else {
                sessionStorage.setItem(key, value);
                localStorage.removeItem(key);
            }
        } catch (e) {
            console.warn('Auth storage set failed', e);
        }
    },
    removeItem: (name: string) => {
        try {
            localStorage.removeItem(name);
            sessionStorage.removeItem(name);
        } catch { /* empty */ }
    },
});
