/**
 * API Endpoints Constants
 * Centralized URL paths for all API calls
 */

export const API_ENDPOINTS = {
    // Auth
    AUTH: {
        LOGIN: '/auth/login',
        REGISTER: '/auth/register',
        REFRESH: '/auth/refresh',
        LOGOUT: '/auth/logout',
    },

    // Users
    USERS: {
        BASE: '/users',
        PROFILE: '/users/profile',
        CHANGE_PASSWORD: '/users/change-password',
    },

    // Addresses
    ADDRESSES: {
        BASE: '/addresses',
        BY_CODE: (code: string) => `/addresses/${code}`,
        SET_DEFAULT: (code: string) => `/addresses/${code}/set-default`,
    },

    // Products
    PRODUCTS: {
        BASE: '/products',
        SEARCH: '/products/search',
        BY_CODE: (code: string) => `/products/${code}`,
    },

    // Categories
    CATEGORIES: {
        BASE: '/categories',
        SEARCH: '/categories/search',
        BY_CODE: (code: string) => `/categories/${code}`,
    },

    // Suppliers
    SUPPLIERS: {
        BASE: '/suppliers',
        SEARCH: '/suppliers/search',
        BY_CODE: (code: string) => `/suppliers/${code}`,
    },

    // Orders
    ORDERS: {
        BASE: '/orders',
        SEARCH: '/orders/search',
        MY_ORDERS: '/orders/my-orders',
        BY_CODE: (code: string) => `/orders/${code}`,
        CANCEL: (code: string) => `/orders/${code}/cancel`,
        STATUS: (code: string) => `/orders/${code}/status`,
    },

    // Quotes
    QUOTES: {
        BASE: '/quotes',
        SEARCH: '/quotes/search',
        BY_CODE: (code: string) => `/quotes/${code}`,
    },

    // Reviews
    REVIEWS: {
        BASE: '/reviews',
        SEARCH: '/reviews/search',
        BY_PRODUCT: (productCode: string) => `/reviews/product/${productCode}`,
    },

    // Payments
    PAYMENTS: {
        BASE: '/payments',
        PROCESS: '/payments/process',
        INITIATE: '/payments/initiate',
        BY_ORDER: (orderCode: string) => `/payments/order/${orderCode}`,
    },

    // Dashboard
    DASHBOARD: {
        STATS: '/dashboard/stats',
        RECENT_ORDERS: '/dashboard/recent-orders',
        TOP_PRODUCTS: '/dashboard/top-products',
        REVENUE_CHART: '/dashboard/revenue-chart',
    },
} as const;

export default API_ENDPOINTS;
