import { createCrudService, API_ENDPOINTS } from './baseService';

export interface BannerDto {
    code: string;
    title: string;
    content?: string;
    linkUrl?: string;
    linkText?: string;
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
    isActive?: boolean;
    priority?: number;
}

export interface UpdateBannerRequest extends Partial<CreateBannerRequest> {
    isActive?: boolean;
}

const bannerCrud = createCrudService<BannerDto, CreateBannerRequest, UpdateBannerRequest>({
    endpoint: API_ENDPOINTS.BANNERS.BASE,
    resourceName: 'Banner'
});

export const bannerService = {
    ...bannerCrud,
};
