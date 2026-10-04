import { ApiResponse, PagedResult } from './api';
import apiClient from './api';

export interface PaymentTransaction {
    id: string;
    orderCode: string;
    userCode: string;
    userName?: string;
    paymentMethod: string;
    amount: number;
    status: 'pending' | 'success' | 'failed' | 'refunded';
    transactionId?: string;
    createdAt: string;
}

export interface PaymentMethodDto {
    code: string;
    name: string;
    description?: string;
    iconUrl?: string;
    sortOrder: number;
    isOnline: boolean;
    isActive: boolean;
    isFixed: boolean;
}

export interface PaymentUrlResponse {
    orderCode: string;
    paymentCode: string;
    paymentUrl: string;
}

export interface PaymentConfirmationResult {
    outcome: 'Confirmed' | 'AlreadyConfirmed' | 'Failed' | 'InvalidSignature' | 'NotFound' | 'InvalidAmount';
    isSuccess: boolean;
    orderCode?: string;
    paymentCode?: string;
    amount: number;
    responseCode?: string;
    message?: string;
}

class PaymentService {
    endpoint = '/payments';

    async getAll(params?: Record<string, unknown>): Promise<ApiResponse<PagedResult<PaymentTransaction>>> {
        return apiClient.get(this.endpoint, { params });
    }

    async updateStatus(paymentCode: string, status: string, transactionId?: string): Promise<ApiResponse<void>> {
        return apiClient.post(`${this.endpoint}/status`, { paymentCode, status, transactionId });
    }

    async create(data: { orderCode: string, paymentMethod: string, amount?: number }): Promise<ApiResponse<{ checkoutUrl?: string }>> {
        return apiClient.post(`${this.endpoint}`, data);
    }

    async createCheckoutUrl(orderCode: string, paymentMethod?: string): Promise<ApiResponse<PaymentUrlResponse>> {
        return apiClient.post(`${this.endpoint}/${orderCode}/checkout`, { paymentMethod });
    }

    async verifyVnPayReturn(params: Record<string, string>): Promise<ApiResponse<PaymentConfirmationResult>> {
        return apiClient.get(`${this.endpoint}/vnpay/return`, { params });
    }

    async verifyMoMoReturn(params: Record<string, string>): Promise<ApiResponse<PaymentConfirmationResult>> {
        return apiClient.get(`${this.endpoint}/momo/return`, { params });
    }

    async getActiveMethods(): Promise<ApiResponse<PaymentMethodDto[]>> {
        return apiClient.get('/paymentmethods/active');
    }

    async getMyPayments(): Promise<ApiResponse<PaymentTransaction[]>> {
        return apiClient.get(`${this.endpoint}/history`);
    }

    async getByOrder(orderCode: string): Promise<ApiResponse<PaymentTransaction>> {
        return apiClient.get(`${this.endpoint}/order/${orderCode}`);
    }
}

export const paymentService = new PaymentService();
