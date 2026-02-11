/**
 * Review Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS, type SearchParams, SearchCondition } from './baseService';
import { apiClient } from './api';
import { REVIEW_LIST_FIELDS } from '@/constants/fieldConstants';

// ============ Types ============
export interface ReviewDto {
    code: string;
    userCode: string;
    userName?: string;
    orderItemCode?: string;
    productCode?: string;
    productName?: string;
    rating: number;
    comment: string;
    createdAt: string;
    isApproved?: boolean;
    parentCode?: string;
    replies?: ReviewDto[];
    userAvatar?: string;
}

export interface ReviewSearchParams {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    isApproved?: boolean;
}

export interface CreateReviewRequest {
    userCode: string;
    orderItemCode?: string;
    productCode?: string;
    rating: number;
    comment: string;
    parentCode?: string;
}

export interface UpdateReviewRequest {
    rating?: number;
    comment?: string;
    isApproved?: boolean;
}

// ============ Service ============
const baseReviewService = createEntityService<ReviewDto, CreateReviewRequest, UpdateReviewRequest>({
    endpoint: API_ENDPOINTS.REVIEWS.BASE,
});

export const reviewService = {
    ...baseReviewService,

    approve: (code: string) =>
        apiClient.post(`${API_ENDPOINTS.REVIEWS.BASE}/${code}/approve`),

    reject: (code: string) =>
        apiClient.post(`${API_ENDPOINTS.REVIEWS.BASE}/${code}/reject`),

    reply: (code: string, reply: string) =>
        apiClient.post(`${API_ENDPOINTS.REVIEWS.BASE}/${code}/reply`, JSON.stringify(reply)), // Backend expects string body

    getByProduct: (productCode: string, params?: SearchParams) =>
        baseReviewService.search({
            ...params,
            filters: [
                ...(params?.filters || []),
                { field: 'ProductCode', value: productCode, operator: SearchCondition.Equal }
            ],
            fields: params?.fields || REVIEW_LIST_FIELDS
        }),
};

export default reviewService;
