/**
 * API Constants
 * Centralized configuration for API calls
 */

// Pagination Defaults
export const PAGINATION = {
    DEFAULT_PAGE_INDEX: 1,
    DEFAULT_PAGE_SIZE: 10,
    MAX_PAGE_SIZE: 100,
    PAGE_SIZE_OPTIONS: [10, 25, 50, 100],
} as const;

// Categories
export const CATEGORIES = {
    DEFAULT_PAGE_SIZE: 100, // Load all categories for dropdowns
} as const;

// Products
export const PRODUCTS = {
    DEFAULT_PAGE_SIZE: 12, // Grid layout typically shows 12 items
    FEATURED_LIMIT: 8,
    FLASH_SALE_LIMIT: 6,
} as const;

// Dashboard
export const DASHBOARD = {
    RECENT_ORDERS_LIMIT: 5,
    TOP_PRODUCTS_LIMIT: 5,
} as const;

// API Response Messages
export const MESSAGES = {
    SUCCESS: {
        CREATE: 'createSuccess',
        UPDATE: 'updateSuccess',
        DELETE: 'deleteSuccess',
        SAVE: 'saveSuccess',
        LOGIN: 'loginSuccess',
    },
    ERROR: {
        CREATE: 'createError',
        UPDATE: 'updateError',
        DELETE: 'deleteError',
        SAVE: 'saveError',
        LOGIN: 'loginError',
        GENERIC: 'error',
    },
} as const;
