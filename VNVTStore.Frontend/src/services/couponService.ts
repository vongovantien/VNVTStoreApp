import { createEntityService, API_ENDPOINTS } from './baseService';
import { apiClient } from './api';

// ============ Types ============
export interface CouponDto {
    code: string;
    promotionCode: string;
    promotionName?: string; // Reference
    usageCount?: number;
    isActive: boolean;
    createdAt?: string;
}

export interface CreateCouponRequest {
    promotionCode: string;
    code: string; // The coupon text code
}

export interface UpdateCouponRequest {
    promotionCode?: string;
    isActive?: boolean;
}

export interface ValidateCouponRequest {
    couponCode: string;
    orderAmount: number;
}

// ============ Service ============
const baseService = createEntityService<CouponDto, CreateCouponRequest, UpdateCouponRequest>({
    endpoint: API_ENDPOINTS.COUPONS.BASE,
});

export const couponService = {
    ...baseService,

    validate: (data: ValidateCouponRequest) =>
        apiClient.post(API_ENDPOINTS.COUPONS.VALIDATE, data),
};

export default couponService;
