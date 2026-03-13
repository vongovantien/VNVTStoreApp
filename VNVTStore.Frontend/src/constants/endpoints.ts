/**
 * API Endpoints Constants
 * Centralized URL paths for all API calls
 */

export const API_ENDPOINTS = {
    // Banners
    BANNERS: {
        BASE: '/banners',
        BY_CODE: (code: string) => `/banners/${code}`,
    },

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

    // Units
    UNITS: {
        BASE: '/units',
        SEARCH: '/units/search',
        BY_CODE: (code: string) => `/units/${code}`,
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

    // Brands
    BRANDS: '/brands',

    // Dashboard
    DASHBOARD: {
        STATS: '/dashboard/stats',
        RECENT_ORDERS: '/dashboard/recent-orders',
        TOP_PRODUCTS: '/dashboard/top-products',
        REVENUE_CHART: '/dashboard/revenue-chart',
    },

    // Promotions
    PROMOTIONS: {
        BASE: '/promotions',
        SEARCH: '/promotions/search',
        BY_CODE: (code: string) => `/promotions/${code}`,
    },

    // Coupons
    COUPONS: {
        BASE: '/coupons',
        SEARCH: '/coupons/search',
        BY_CODE: (code: string) => `/coupons/${code}`,
        VALIDATE: '/coupons/validate',
    },

    // News/Posts
    NEWS: {
        BASE: '/news',
        SEARCH: '/news/search',
        BY_CODE: (code: string) => `/news/${code}`,
    },

    // RBAC
    ROLES: {
        BASE: '/roles',
        SEARCH: '/roles/search',
        BY_CODE: (code: string) => `/roles/${code}`,
        PERMISSIONS: '/roles/permissions',
    },

    // Audit Logs
    AUDIT_LOGS: {
        BASE: '/auditlogs',
        SEARCH: '/auditlogs/search',
    },

    PERMISSIONS: {
        BASE: '/permissions',
        SEARCH: '/permissions/search',
    },
    MENUS: {
        BASE: '/menus',
        SEARCH: '/menus/search',
        BY_CODE: (code: string) => `/menus/${code}`,
    },
} as const;

export default API_ENDPOINTS;
