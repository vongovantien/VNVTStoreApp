/**
 * Order Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS } from './baseService';

// ============ Types ============
export interface OrderItemDto {
    id?: string;
    code: string;
    productCode: string;
    productName: string;
    productImage: string;
    priceAtOrder: number;
    discountAmount: number;
    quantity: number;
    size?: string;
    color?: string;
}

export interface OrderDto {
    code: string;
    userCode: string;
    status: string;
    totalAmount: number;
    finalAmount: number;
    orderDate: string;
    orderItems: OrderItemDto[];
    shippingFee: number;
    discountAmount: number;
    note?: string;
    paymentMethod?: string;
    paymentStatus?: string;
    shippingName?: string;
    shippingPhone?: string;
    shippingAddress?: string;
    city?: string;
    customerName?: string;
    customerPhone?: string;
    customerEmail?: string;
}

export interface GuestCartItemDto {
    productCode: string;
    quantity: number;
    size?: string;
    color?: string;
}

export interface CreateOrderRequest {
    fullName: string;
    email?: string;
    phone: string;
    address: string;
    city: string;
    district: string;
    ward: string;
    note?: string;
    paymentMethod: string;
    couponCode?: string;
    items?: GuestCartItemDto[];
}

export interface UpdateOrderRequest {
    status?: string;
    note?: string;
}

// ============ Service ============
import { SearchParams } from './baseService';
import apiClient from './api';

export interface OrderStats {
    total: number;
    pending: number;
    shipping: number;
    delivered: number;
}

const baseService = createEntityService<OrderDto, CreateOrderRequest, UpdateOrderRequest>({
    endpoint: API_ENDPOINTS.ORDERS.BASE,
});

export const orderService = {
    ...baseService,
    getMyOrders: (params: SearchParams) => baseService.search(params),
    // Override create to send raw DTO (not wrapped in PostObject, as expected by OrdersController)
    create: async (data: CreateOrderRequest) => {
        const response = await apiClient.post<OrderDto>(API_ENDPOINTS.ORDERS.BASE, data);
        if (!response.success) {
            throw new Error(response.message || 'Create failed');
        }
        return response;
    },
    verify: async (token: string) => {
        return await apiClient.get<string>(`${API_ENDPOINTS.ORDERS.BASE}/verify`, { params: { token } });
    },
    getStats: async (): Promise<OrderStats> => {
        const response = await apiClient.get<OrderStats>(`${API_ENDPOINTS.ORDERS.BASE}/stats`);
        if (!response.success && response.message?.includes('404')) {
            return { total: 0, pending: 0, shipping: 0, delivered: 0 }
        }
        return response.data || { total: 0, pending: 0, shipping: 0, delivered: 0 };
    },
    updateStatus: async (code: string, status: string) => {
        const response = await apiClient.put<OrderDto>(`${API_ENDPOINTS.ORDERS.BASE}/${code}/status`, { status });
        if (!response.success) {
            throw new Error(response.message || 'Update status failed');
        }
        return response;
    }
};

export default orderService;
