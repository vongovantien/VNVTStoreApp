/**
 * Order Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS } from './baseService';

// ============ Types ============
export interface OrderItemDto {
    productCode: string;
    productName: string;
    productImage: string;
    price: number;
    quantity: number;
}

export interface OrderDto {
    code: string;
    userCode: string;
    status: string;
    totalAmount: number;
    finalAmount: number;
    createdAt: string;
    orderItems: OrderItemDto[];
    shippingFee: number;
    discountAmount: number;
    note?: string;
    paymentMethod?: string;
    paymentStatus?: string;
    shippingName?: string;
    shippingPhone?: string;
    shippingAddress?: string;
}

export interface CreateOrderRequest {
    fullName: string;
    phone: string;
    address: string;
    city: string;
    district: string;
    ward: string;
    note?: string;
    paymentMethod: string;
    couponCode?: string;
}

export interface UpdateOrderRequest {
    status?: string;
    note?: string;
}

// ============ Service ============
import { SearchParams } from './baseService';
import apiClient from './api'; // Import apiClient directly

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
    }
};

export default orderService;
