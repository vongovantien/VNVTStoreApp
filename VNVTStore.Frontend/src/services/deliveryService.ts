import apiClient from './api';
import { Delivery, ApiResponse, DeliveryHistory } from '@/types';

class DeliveryService {
    async assignShipper(orderCode: string, payload: any): Promise<ApiResponse<Delivery>> {
        return apiClient.post(`/deliveries/order/${orderCode}/assign`, payload);
    }

    async updateStatus(deliveryCode: string, payload: { status: string; note?: string; location?: string }): Promise<ApiResponse<Delivery>> {
        return apiClient.put(`/deliveries/${deliveryCode}/status`, payload);
    }

    async getByOrderCode(orderCode: string): Promise<ApiResponse<Delivery>> {
        return apiClient.get(`/deliveries/order/${orderCode}`);
    }

    async getHistory(deliveryCode: string): Promise<ApiResponse<DeliveryHistory[]>> {
        return apiClient.get(`/deliveries/${deliveryCode}/history`);
    }
}

export const deliveryService = new DeliveryService();
