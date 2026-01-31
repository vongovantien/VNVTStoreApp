/**
 * Review Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS, type PagedResult } from './baseService';
import { apiClient } from './api';

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
    adminReply?: string;
    createdAt: string;
    isApproved?: boolean;
}

export interface ReviewSearchParams {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    isApproved?: boolean;
}

export interface CreateReviewRequest {
    orderItemCode: string;
    rating: number;
    comment: string;
    userCode: string;
}

export interface UpdateReviewRequest {
    rating?: number;
    comment?: string;
    isApproved?: boolean;
    adminReply?: string;
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

    getByProduct: (productCode: string, pageIndex: number = 1, pageSize: number = 10) =>
        apiClient.get<PagedResult<ReviewDto>>(`${API_ENDPOINTS.REVIEWS.BY_PRODUCT(productCode)}?pageIndex=${pageIndex}&pageSize=${pageSize}`),
};

export default reviewService;
