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
    orderTotal?: number;
    orderAmount?: number;
}

export interface CouponValidationData {
    isValid: boolean;
    errors: string[];
    discountAmount: number;
    finalAmount: number;
    couponDetails?: {
        code: string;
        promotionName?: string;
        description?: string;
        discountType: string;
        discountValue: number;
        maxDiscountAmount?: number;
        minOrderAmount?: number;
    } | null;
}

// ============ Service ============
const baseService = createEntityService<CouponDto, CreateCouponRequest, UpdateCouponRequest>({
    endpoint: API_ENDPOINTS.COUPONS.BASE,
});

export const couponService = {
    ...baseService,

    validate: (data: ValidateCouponRequest) =>
        apiClient.post<{ success: boolean; message?: string; data: CouponValidationData }>(
            API_ENDPOINTS.COUPONS.VALIDATE,
            {
                couponCode: data.couponCode,
                orderTotal: data.orderTotal ?? data.orderAmount ?? 0,
                orderAmount: data.orderAmount ?? data.orderTotal ?? 0,
            }
        ),
};

export default couponService;
