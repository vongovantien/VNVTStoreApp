/**
 * Quote Service
 * Uses only baseService CRUD methods
 */

import { createCrudService, API_ENDPOINTS } from './baseService';

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
}

export interface UpdateQuoteRequest {
    status?: string;
    quotedPrice?: number;
    note?: string;
}

// ============ Service ============
export const quoteService = createCrudService<QuoteDto, CreateQuoteRequest, UpdateQuoteRequest>({
    endpoint: API_ENDPOINTS.QUOTES.BASE,
    resourceName: 'Quote'
});

export default quoteService;
