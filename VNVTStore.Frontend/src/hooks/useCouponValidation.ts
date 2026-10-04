import { useMutation } from '@tanstack/react-query';
import { couponService, type ValidateCouponRequest, type CouponValidationData } from '@/services/couponService';

export type { ValidateCouponRequest, CouponValidationData };

/**
 * Hook validate coupon — GỌI API BACKEND (SINGLE SOURCE OF TRUTH)
 * Không duplicate validation logic ở frontend
 */
export function useCouponValidation() {
    return useMutation({
        mutationFn: async (request: ValidateCouponRequest): Promise<CouponValidationData> => {
            const response = await couponService.validate(request);
            if (response.data && response.data.data) {
                return response.data.data;
            }
            return {
                isValid: false,
                errors: [response.data?.message || 'Không thể xác thực mã giảm giá'],
                discountAmount: 0,
                finalAmount: request.orderTotal ?? request.orderAmount ?? 0,
            };
        },
    });
}
