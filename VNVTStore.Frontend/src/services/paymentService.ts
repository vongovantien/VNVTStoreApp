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

class PaymentService {
    endpoint = '/payment';

    async getAll(params?: Record<string, unknown>): Promise<ApiResponse<PagedResult<PaymentTransaction>>> {
        return apiClient.get(this.endpoint, { params });
    }

    async updateStatus(paymentCode: string, status: string, transactionId?: string): Promise<ApiResponse<void>> {
        return apiClient.post(`${this.endpoint}/status`, { paymentCode, status, transactionId });
    }

    async create(data: { orderCode: string, amount: number, paymentMethod: string }): Promise<ApiResponse<{ checkoutUrl: string }>> {
        return apiClient.post(`${this.endpoint}`, data);
    }

    async getMyPayments(): Promise<ApiResponse<PaymentTransaction[]>> {
        return apiClient.get(`${this.endpoint}/history`);
    }
}

export const paymentService = new PaymentService();
