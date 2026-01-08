import { apiClient, type ApiResponse } from './api';

export interface PaymentDto {
    id: string;
    orderCode: string;
    method: string;
    status: string;
    amount: number;
    paymentDate: string;
    transactionId?: string;
}

export interface ProcessPaymentRequest {
    orderCode: string;
    method: string;
    amount: number;
}

export const paymentService = {
    async processPayment(data: ProcessPaymentRequest): Promise<ApiResponse<boolean>> {
        return apiClient.post<boolean>('/payments/process', data);
    },

    async getPaymentByOrder(orderCode: string): Promise<ApiResponse<PaymentDto>> {
        return apiClient.get<PaymentDto>(`/payments/order/${orderCode}`);
    },

    // For VNPAY/Momo, likely need detailed initiate payment response (URL)
    async initiatePayment(orderCode: string, method: string): Promise<ApiResponse<string>> {
        // Assuming backend returns a payment URL for redirect
        return apiClient.post<string>('/payments/initiate', { orderCode, method });
    }
};

export default paymentService;
