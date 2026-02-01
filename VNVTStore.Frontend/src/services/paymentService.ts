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
    endpoint = '/payments';

    // Admin: Get all payments (Mocked for now as backend missing)
    async getAll(params?: any): Promise<ApiResponse<PagedResult<PaymentTransaction>>> {
        // Mock delay
        await new Promise(resolve => setTimeout(resolve, 800));

        const transactions: PaymentTransaction[] = Array(15).fill(null).map((_, i) => ({
            id: `pay-${i + 1000}`,
            orderCode: `ORD-${2023000 + i}`,
            userCode: `USR-${i}`,
            userName: `User ${i + 1}`,
            paymentMethod: i % 3 === 0 ? 'VNPAY' : i % 3 === 1 ? 'MOMO' : 'COD',
            amount: (i + 1) * 500000,
            status: ['pending', 'success', 'failed', 'refunded'][i % 4] as any,
            transactionId: `TRX-${Date.now()}-${i}`,
            createdAt: new Date().toISOString()
        }));

        return {
            success: true,
            message: 'Success',
            data: {
                items: transactions,
                totalItems: 50,
                pageIndex: 1,
                pageSize: 15,
                totalPages: 4,
                hasPreviousPage: false,
                hasNextPage: true
            },
            statusCode: 200
        };
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
