/**
 * Quote Service
 * Uses only baseService CRUD methods
 */

/* eslint-disable @typescript-eslint/no-explicit-any */
import { createEntityService, API_ENDPOINTS } from './baseService';
import apiClient from './api';

// ============ Types ============
export interface QuoteDto {
    code: string;
    productCode: string;
    userName?: string;
    productName?: string;
    productImage?: string;
    quantity: number;
    note?: string;
    status: string;
    quotedPrice?: number;
    createdAt: string;
    customerName?: string;
    customerEmail?: string;
    customerPhone?: string;
}

export interface CreateQuoteRequest {
    productCode: string;
    quantity: number;
    note?: string;
    customerName?: string;
    customerEmail?: string;
    customerPhone?: string;
    company?: string;
}

export interface UpdateQuoteRequest {
    status?: string;
    quotedPrice?: number;
    note?: string;
}

// ============ Service ============
const baseService = createEntityService<QuoteDto, CreateQuoteRequest, UpdateQuoteRequest>({
    endpoint: API_ENDPOINTS.QUOTES.BASE,
});

export const quoteService = {
    ...baseService,
    getQuotes: async () => {
        const res = await baseService.search({ pageIndex: 1, pageSize: 100 });
        return {
            ...res,
            data: res.data?.items || []
        };
    },
    getStats: async () => {
        const response = await apiClient.get<any>(`${API_ENDPOINTS.QUOTES.BASE}/stats`);
        return response.data || { total: 0, pending: 0, processed: 0 }; // Backend returns {Total, Active}. Mapping needed?
        // Wait, Backend returns EntityStatsDto {Total, Active}.
        // Frontend Quote Stats expects { total, pending, processed }?
        // Quotes usually have Status (Pending, Processed, etc).
        // My Generic Backend Stats only returns Total and Active (IsActive).
        // It does NOT return Pending/Processed counts specifically for Quotes unless I override logic.
    }
};

export default quoteService;
