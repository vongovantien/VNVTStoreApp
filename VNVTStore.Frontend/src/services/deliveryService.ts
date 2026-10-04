import apiClient from './api';
import { Delivery, ApiResponse, DeliveryHistory, PagedResult } from '@/types';

class DeliveryService {
    async getDeliveries(params?: {
        status?: string;
        search?: string;
        shipperCode?: string;
        pageIndex?: number;
        pageSize?: number;
    }): Promise<ApiResponse<PagedResult<Delivery>>> {
        return apiClient.get('/deliveries', { params });
    }

    async getMyTasks(params?: { status?: string; pageIndex?: number; pageSize?: number }): Promise<ApiResponse<PagedResult<Delivery>>> {
        return apiClient.get('/deliveries/my-tasks', { params });
    }

    async assignShipper(orderCode: string, payload: {
        shipperCode?: string;
        shipperName?: string;
        shipperPhone?: string;
        carrierName?: string;
        trackingNumber?: string;
        estimatedDeliveryDate?: string;
        note?: string;
    }): Promise<ApiResponse<Delivery>> {
        return apiClient.post(`/deliveries/order/${orderCode}`, payload);
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
export default deliveryService;
