/**
 * Permission Constants
 * Must match backend Permissions.cs
 */
export const PERMISSIONS = {
    DASHBOARD: {
        VIEW: 'Dashboard.View',
    },
    CATALOG: {
        VIEW_PRODUCTS: 'Catalog.ViewProducts',
        MANAGE_PRODUCTS: 'Catalog.ManageProducts',
        VIEW_CATEGORIES: 'Catalog.ViewCategories',
        MANAGE_CATEGORIES: 'Catalog.ManageCategories',
        VIEW_BRANDS: 'Catalog.ViewBrands',
        MANAGE_BRANDS: 'Catalog.ManageBrands',
    },
    SALES: {
        VIEW_ORDERS: 'Sales.ViewOrders',
        MANAGE_ORDERS: 'Sales.ManageOrders',
        VIEW_QUOTES: 'Sales.ViewQuotes',
        MANAGE_QUOTES: 'Sales.ManageQuotes',
    },
    CUSTOMERS: {
        VIEW: 'Customers.View',
        MANAGE: 'Customers.Manage',
    },
    MARKETING: {
        MANAGE_PROMOTIONS: 'Marketing.ManagePromotions',
        MANAGE_COUPONS: 'Marketing.ManageCoupons',
        MANAGE_BANNERS: 'Marketing.ManageBanners',
    },
    CONTENT: {
        MANAGE_NEWS: 'Content.ManageNews',
    },
    SETTINGS: {
        VIEW: 'Settings.View',
        MANAGE_ROLES: 'Settings.ManageRoles',
        MANAGE_SYSTEM: 'Settings.ManageSystem',
    },
} as const;

export default PERMISSIONS;
