/**
 * Quote Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS } from './baseService';

// ============ Types ============
export interface QuoteDto {
    code: string;
    productCode: string;
    productName?: string;
    productImage?: string;
    quantity: number;
    note?: string;
    status: string;
    quotedPrice?: number;
    createdAt: string;
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
    }
};

export default quoteService;
