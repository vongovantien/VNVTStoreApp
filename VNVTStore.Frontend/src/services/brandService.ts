import createEntityService from './baseService';
import { API_ENDPOINTS } from '@/constants';
import apiClient, { type ApiResponse, type PagedResult } from './api';

export interface BrandDto {
    code: string;
    name: string;
    description?: string;
    logoUrl?: string;
    isActive: boolean;
}

export interface CreateBrandRequest {
    name: string;
    description?: string;
    logoUrl?: string;
}

export interface UpdateBrandRequest {
    name?: string;
    description?: string;
    logoUrl?: string;
    isActive?: boolean;
}

const baseBrandService = createEntityService<BrandDto, CreateBrandRequest, UpdateBrandRequest>({
    endpoint: API_ENDPOINTS.BRANDS,
});

export const brandService = {
    ...baseBrandService,
    async getActiveBrands(): Promise<ApiResponse<PagedResult<BrandDto>>> {
        return this.search({
            pageIndex: 1,
            pageSize: 100,
            filters: [{ field: 'IsActive', value: 'true' }]
        });
    },
    getStats: async () => {
        const response = await apiClient.get<{ total: number; active: number }>(`${API_ENDPOINTS.BRANDS}/stats`);
        return response.data || { total: 0, active: 0 };
    },

    import: async (file: File): Promise<ApiResponse<number>> => {
        const formData = new FormData();
        formData.append('file', file);
        return apiClient.post(`${API_ENDPOINTS.BRANDS}/import`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
    },

    getTemplate: (): string => {
        return `${API_ENDPOINTS.BRANDS}/template`;
    }
};
