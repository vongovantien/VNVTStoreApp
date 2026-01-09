/**
 * Order Service
 * Uses only baseService CRUD methods
 */

import { createCrudService, API_ENDPOINTS } from './baseService';

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
}

export interface UpdateOrderRequest {
    status?: string;
    note?: string;
}

// ============ Service ============
export const orderService = createCrudService<OrderDto, CreateOrderRequest, UpdateOrderRequest>({
    endpoint: API_ENDPOINTS.ORDERS.BASE,
    resourceName: 'Order'
});

export default orderService;
