import { orderService, CreateOrderRequest, OrderDto } from '@/services/orderService';
import { CustomerInfo, OrderItem } from '../types';

export interface ICheckoutRepository {
    submitOrder(customer: CustomerInfo, items: OrderItem[]): Promise<OrderDto>;
}

export class CheckoutRepository implements ICheckoutRepository {
    async submitOrder(customer: CustomerInfo, items: OrderItem[]): Promise<OrderDto> {
        const payload: CreateOrderRequest = {
            fullName: customer.fullName,
            phone: customer.phoneNumber,
            address: customer.address,
            city: 'Unknown',
            district: 'Unknown',
            ward: 'Unknown',
            note: customer.note,
            paymentMethod: 'COD', // Default for now
            items: items.map(i => ({
                productCode: i.productId,
                quantity: i.quantity
            }))
        };

        // This returns ApiResponse<OrderDto>
        const response = await orderService.create(payload);

        // Return raw data or mapped object
        if (response.success && response.data) {
            return response.data;
        }

        throw new Error(response.message || 'Order creation failed');
    }
}
