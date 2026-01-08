import { apiClient, type ApiResponse, type PagedResult } from './api';

// ============ API DTOs ============
export interface OrderItemDto {
    productCode: string;
    productName: string;
    productImage: string;
    price: number;
    quantity: number;
}

export interface OrderDto {
    code: string;
    userCode: string; // "CreatedBy"
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
    // Address info might be separate or embedded depending on backend
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

export interface CancelOrderRequest {
    reason: string;
}

export interface OrderFilterRequest {
    pageIndex: number;
    pageSize: number;
    status?: string;
}

// ============ Service ============
export const orderService = {
    async createOrder(data: CreateOrderRequest): Promise<ApiResponse<string>> { // Returns OrderCode
        return apiClient.post<string>('/orders', data);
    },

    async getMyOrders(filter: OrderFilterRequest): Promise<ApiResponse<PagedResult<OrderDto>>> {
        // Backend `GetMyOrdersQuery` takes Status filter? 
        // Assuming PagedResult from typical query pattern or simple list.
        // If backend returns List<OrderDto>, we wrap it?
        // Let's assume backend matches standard pattern.
        // Backend Controller: `[HttpGet] GetMyOrders([FromQuery] string? status)`
        // And it likely returns List or Paged. 
        // Handlers `GetMyOrdersQuery` returns `Result<List<OrderDto>>` based on code reading earlier.
        // So it's not paged in backend yet?
        // Wait, earlier I saw `PagedResult` references.
        // Let's check `OrderHandlers` if possible. But for now assuming List.
        const query = new URLSearchParams();
        if (filter.status) query.append('status', filter.status);

        return apiClient.get<PagedResult<OrderDto>>(`/orders/my-orders?${query.toString()}`);
    },

    async getOrderById(orderCode: string): Promise<ApiResponse<OrderDto>> {
        return apiClient.get<OrderDto>(`/orders/${orderCode}`);
    },

    async cancelOrder(orderCode: string, reason: string): Promise<ApiResponse<boolean>> {
        return apiClient.put<boolean>(`/orders/${orderCode}/cancel`, { reason });
    }
};

export default orderService;
