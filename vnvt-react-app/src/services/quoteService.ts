import { apiClient, ApiResponse, RequestDTO } from './apiClient';
import { QuoteRequest } from '@/types';


export interface CreateQuoteRequest {
    productId: string;
    productName: string;
    productImage: string;
    customerName: string;
    customerEmail: string;
    customerPhone: string;
    company?: string;
    quantity: number;
    note?: string;
}

const STORAGE_KEY = 'vnvt-quotes';

// Real Service using API
export const quoteService = {
    async createQuote(data: CreateQuoteRequest): Promise<ApiResponse<QuoteRequest>> {
        const request: RequestDTO<CreateQuoteRequest> = {
            postObject: data,
        };
        // Map Frontend Request to Backend Command fields if needed
        // CreateQuoteCommand: ProductCode, Quantity, Note.
        // Frontend data has more (customerName etc). Backend infers user from Token.
        // We only send ProductCode, Quantity, Note.

        const command = {
            productCode: data.productId,
            quantity: data.quantity,
            note: data.note
        };

        const response = await apiClient.post<QuoteDto>('/quotes', command);

        if (response.success && response.data) {
            return {
                ...response,
                data: mapQuoteDtoToQuoteRequest(response.data)
            };
        }
        return { ...response, data: null as any };
    },

    async getQuotes(): Promise<ApiResponse<QuoteRequest[]>> {
        const response = await apiClient.get<QuoteDto[]>('/quotes');
        if (response.success && response.data) {
            return {
                ...response,
                data: response.data.map(mapQuoteDtoToQuoteRequest)
            };
        }
        return { ...response, data: [] };
    },

    async updateQuote(id: string, updates: Partial<QuoteRequest>): Promise<ApiResponse<QuoteRequest>> {
        // Not implemented in Backend yet? Or use generic update if available.
        // For now, return mock or error.
        return { success: false, status: 501, message: 'Update not implemented yet', data: null as any };
    }
};

// Mapper
import { QuoteDto } from '@/services/productService'; // Reuse or define locally? 
// DTO definition in frontend might be needed if not imported.
interface QuoteDto {
    code: string;
    productName?: string;
    productImage?: string;
    status: string;
    createdAt: string;
    // ... other fields
    productCode: string;
    quantity: number;
    note?: string;
    quotedPrice?: number;
}

function mapQuoteDtoToQuoteRequest(dto: QuoteDto): QuoteRequest {
    return {
        id: dto.code,
        productName: dto.productName || 'Unknown Product',
        productImage: dto.productImage || '',
        productId: dto.productCode,
        quantity: dto.quantity,
        note: dto.note,
        status: dto.status as any,
        quotedPrice: dto.quotedPrice,
        createdAt: dto.createdAt,
        customer: { name: 'Me', email: '', phone: '' } // Placeholder as API doesn't return user details in QuoteDto yet
    };
}

