import { ApiResponse } from './api';
import apiClient from './api';
import createEntityService, { SearchParams } from './baseService';

export interface Promotion {
    code: string;
    name: string;
    description?: string;
    discountType: 'PERCENTAGE' | 'AMOUNT' | 'FIXED_PRICE';
    discountValue: number;
    minOrderAmount?: number;
    maxDiscountAmount?: number;
    startDate: string;
    endDate: string;
    usageLimit?: number;
    isActive: boolean;
    productCodes?: string[];
}

export interface CreatePromotionRequest {
    code: string;
    name: string;
    description?: string;
    discountType: string;
    discountValue: number;
    minOrderAmount?: number;
    maxDiscountAmount?: number;
    startDate: string; // ISO string
    endDate: string; // ISO string
    usageLimit?: number;
    isActive: boolean;
    productCodes?: string[];
}

export interface UpdatePromotionRequest extends CreatePromotionRequest { }

export interface PromotionFilter extends SearchParams {
    activeOnly?: boolean;
}

const baseService = createEntityService<Promotion, CreatePromotionRequest, UpdatePromotionRequest>({
    endpoint: '/promotions',
    resourceName: 'Promotion'
});

export const promotionService = {
    ...baseService,

    // Use baseService.search which calls POST /search endpoint
    getAll: async (filter: PromotionFilter = {}) => {
        return baseService.search({
            pageIndex: filter.pageIndex,
            pageSize: filter.pageSize || 100,
            search: filter.search,
            searchField: filter.searchField,
            sortBy: filter.sortBy,
            sortDesc: filter.sortDesc,
            filters: filter.activeOnly
                ? [{ field: 'isActive', value: true }]
                : filter.filters,
            fields: filter.fields
        });
    },

    getFlashSales: async () => {
        const response = await apiClient.get<ApiResponse<Promotion[]>>('/promotions/flash-sale');
        return response.data;
    },

    import: async (file: File) => {
        const formData = new FormData();
        formData.append('file', file);
        const response = await apiClient.post<ApiResponse<number>>('/promotions/import', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    }
};
