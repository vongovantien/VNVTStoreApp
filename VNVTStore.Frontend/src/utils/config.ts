/**
 * Central configuration for API and application settings
 */

const DEFAULT_API_URL = 'http://localhost:5000/api/v1';
const STORAGE_KEY = 'vnvt_api_url';

export const getApiUrl = (): string => {
    // 1. Check environment variable first (Vite build time)
    const envUrl = import.meta.env.VITE_API_URL;
    
    // 2. Check localStorage for manual overrides
    const savedUrl = (typeof localStorage !== 'undefined') ? localStorage.getItem(STORAGE_KEY) : null;
    
    // If we have a saved URL but it's NOT the target port (5000), clear it and use defaults
    if (savedUrl && !savedUrl.includes(':5000')) {
        localStorage.removeItem(STORAGE_KEY);
        return envUrl || DEFAULT_API_URL;
    }

    if (savedUrl && savedUrl.trim() !== '') {
        return savedUrl.trim();
    }
    
    return envUrl || DEFAULT_API_URL;
};

/**
 * Update the API URL and persist it to localStorage.
 * Call this from the UI or console to change backends on the fly.
 */
export const setApiUrl = (url: string | null): void => {
    if (typeof localStorage === 'undefined') return;

    if (!url) {
        localStorage.removeItem(STORAGE_KEY);
        console.log('✅ API URL reset to default (environment variable).');
    } else {
        // Ensure trailing slash if needed, or consistent format
        let formattedUrl = url.trim();
        // Remove trailing slash for consistency if preferred
        if (formattedUrl.endsWith('/')) {
            formattedUrl = formattedUrl.slice(0, -1);
        }
        
        localStorage.setItem(STORAGE_KEY, formattedUrl);
        console.log(`✅ API URL updated to: ${formattedUrl}`);
    }
    
    // Most application logic uses the API URL once at startup, 
    // so a reload is often necessary to apply the change globally.
    console.log('🔄 Reloading page to apply changes...');
    window.location.reload();
};

/**
 * Helper to get the root URL (without /api/v1) for serving media files
 */
export const getApiRoot = (): string => {
    const apiUrl = getApiUrl();
    return apiUrl.replace(/\/api\/v1\/?$/, '');
};

declare global {
  interface Window {
    VNVT_CONFIG: {
        getApiUrl: () => string;
        setApiUrl: (url: string | null) => void;
        getApiRoot: () => string;
    };
  }
}

// Expose to window for easy debugging/switching in browser console
if (typeof window !== 'undefined') {
    window.VNVT_CONFIG = {
        getApiUrl,
        setApiUrl,
        getApiRoot
    };
}
