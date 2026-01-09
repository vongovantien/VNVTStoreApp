/**
 * Payment Service
 * Uses only baseService CRUD methods
 */

import { createCrudService, API_ENDPOINTS } from './baseService';

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
    method: string;
    amount: number;
}

export interface UpdatePaymentRequest {
    status?: string;
    transactionId?: string;
}

// ============ Service ============
export const paymentService = createCrudService<PaymentDto, CreatePaymentRequest, UpdatePaymentRequest>({
    endpoint: API_ENDPOINTS.PAYMENTS.BASE,
    resourceName: 'Payment'
});

export default paymentService;
