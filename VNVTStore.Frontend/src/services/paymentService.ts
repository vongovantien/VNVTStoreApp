/**
 * Payment Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS } from './baseService';
import { apiClient } from './api';

// ============ Types ============
export interface PaymentDto {
    code: string;
    orderCode: string;
    method: string;
    status: string;
    amount: number;
    paymentDate: string;
    transactionId?: string;
}

export interface CreatePaymentRequest {
    orderCode: string;
    paymentMethod: string;
    amount: number;
}

export interface UpdatePaymentRequest {
    status?: string;
    transactionId?: string;
}

// ============ Service ============
// ============ Service ============
const baseService = createEntityService<PaymentDto, CreatePaymentRequest, UpdatePaymentRequest>({
    endpoint: API_ENDPOINTS.PAYMENTS.BASE,
});

export const paymentService = {
    ...baseService,
    // Override create to send flat payload (no PostObject wrapper) as required by PaymentsController
    create: async (data: CreatePaymentRequest) => {
        const response = await apiClient.post<PaymentDto>(API_ENDPOINTS.PAYMENTS.BASE, data);
        if (!response.success) {
            throw new Error(response.message || 'Payment processing failed');
        }
        return response;
    }
};

export default paymentService;
