/**
 * Global Search Service — tìm kiếm nhanh products, orders, customers
 */

import api from './api';

export interface GlobalSearchResult {
    products: Array<{
        code: string;
        name: string;
        price: number;
        imageUrl?: string;
        stock: number;
    }>;
    orders: Array<{
        code: string;
        orderNumber: string;
        customerName: string;
        totalAmount: number;
        status: string;
        createdAt: string;
    }>;
    customers: Array<{
        code: string;
        fullName: string;
        email: string;
        phone?: string;
        totalSpent?: number;
    }>;
}

const extractItems = (data: unknown): any[] => {
    if (!data) return [];
    if (Array.isArray(data)) return data;
    const obj = data as Record<string, unknown>;
    if (Array.isArray(obj.items)) return obj.items;
    if (Array.isArray(obj.data)) return obj.data;
    const nested = obj.data as Record<string, unknown> | undefined;
    if (nested && Array.isArray(nested.items)) return nested.items;
    return [];
};

/**
 * Tìm kiếm tổng hợp qua Unified System Search API
 */
export const globalSearch = async (query: string): Promise<GlobalSearchResult> => {
    const q = query.trim();
    if (!q) {
        return { products: [], orders: [], customers: [] };
    }

    try {
        // 1. Try unified system multi-entity search
        const res = await api.get('/system/search', { params: { q } });
        if (res.data?.success && res.data.data) {
            return {
                products: extractItems(res.data.data.products),
                orders: extractItems(res.data.data.orders),
                customers: extractItems(res.data.data.customers),
            };
        }
    } catch {
        // fallback
    }

    try {
        const [productsRes, ordersRes, customersRes] = await Promise.allSettled([
            api.post('/products/search', { pageIndex: 1, pageSize: 5, search: q }),
            api.post('/orders/search', { pageIndex: 1, pageSize: 5, search: q }),
            api.post('/users/search', { pageIndex: 1, pageSize: 5, search: q })
        ]);

        return {
            products: productsRes.status === 'fulfilled' ? extractItems(productsRes.value.data?.data) : [],
            orders: ordersRes.status === 'fulfilled' ? extractItems(ordersRes.value.data?.data) : [],
            customers: customersRes.status === 'fulfilled' ? extractItems(customersRes.value.data?.data) : []
        };
    } catch (error) {
        console.error('[globalSearch] Error:', error);
        return { products: [], orders: [], customers: [] };
    }
};

export default globalSearch;
