import { createEntityService, API_ENDPOINTS } from './baseService';
import apiClient, { type ApiResponse } from './api';

export interface BannerDto {
    code: string;
    title: string;
    content?: string;
    linkUrl?: string;
    linkText?: string;
    imageURL?: string;
    isActive: boolean;
    priority: number;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateBannerRequest {
    title: string;
    content?: string;
    linkUrl?: string;
    linkText?: string;
    imageURL?: string;
    isActive?: boolean;
    priority?: number;
}

export interface UpdateBannerRequest extends Partial<CreateBannerRequest> {
    isActive?: boolean;
}

const bannerServiceBase = createEntityService<BannerDto, CreateBannerRequest, UpdateBannerRequest>({
    endpoint: API_ENDPOINTS.BANNERS.BASE,
});

export const bannerService = {
    ...bannerServiceBase,
    import: async (file: File): Promise<ApiResponse<number>> => {
        const formData = new FormData();
        formData.append('file', file);
        return apiClient.post(`${API_ENDPOINTS.BANNERS.BASE}/import`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
    },

    getTemplate: (): string => {
        return `${API_ENDPOINTS.BANNERS.BASE}/template`;
    }
};
