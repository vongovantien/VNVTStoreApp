import { createEntityService, API_ENDPOINTS } from './baseService';

// ============ Types ============
export interface NewsDto {
    code: string;
    title: string;
    summary?: string;
    content?: string;
    thumbnail?: string;
    author?: string;
    publishedAt?: string;
    isActive: boolean;
    metaTitle?: string;
    metaDescription?: string;
    metaKeywords?: string;
    slug?: string;
    createdAt?: string;
}

export interface CreateNewsRequest {
    title: string;
    summary?: string;
    content?: string;
    thumbnail?: string;
    author?: string;
    isActive?: boolean;
    metaTitle?: string;
    metaDescription?: string;
    metaKeywords?: string;
    slug?: string;
}

export type UpdateNewsRequest = Partial<CreateNewsRequest>;

// ============ Service ============
export const newsService = createEntityService<NewsDto, CreateNewsRequest, UpdateNewsRequest>({
    endpoint: API_ENDPOINTS.NEWS.BASE,
});

export default newsService;
