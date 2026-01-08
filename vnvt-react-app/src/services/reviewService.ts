import { apiClient, type ApiResponse } from './api';

export interface ReviewDto {
    id: string; // Code
    productCode: string;
    userCode: string;
    userName: string; // If mapped
    rating: number;
    comment: string;
    createdAt: string;
}

export interface CreateReviewRequest {
    productCode: string;
    orderItemCode: string; // Required for verification
    rating: number;
    comment: string;
}

export const reviewService = {
    async getProductReviews(productCode: string): Promise<ApiResponse<ReviewDto[]>> {
        return apiClient.get<ReviewDto[]>(`/reviews/product/${productCode}`);
    },

    async createReview(data: CreateReviewRequest): Promise<ApiResponse<string>> {
        return apiClient.post<string>('/reviews', data);
    }
};

export default reviewService;
