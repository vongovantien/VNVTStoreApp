/**
 * Review Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS } from './baseService';

// ============ Types ============
export interface ReviewDto {
    code: string;
    productCode: string;
    userCode: string;
    userName?: string;
    rating: number;
    comment: string;
    createdAt: string;
    status?: string;
}

export interface CreateReviewRequest {
    productCode: string;
    orderItemCode: string;
    rating: number;
    comment: string;
}

export interface UpdateReviewRequest {
    status?: string;
}

// ============ Service ============
export const reviewService = createEntityService<ReviewDto, CreateReviewRequest, UpdateReviewRequest>({
    endpoint: API_ENDPOINTS.REVIEWS.BASE,
});

export default reviewService;
