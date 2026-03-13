import { CustomerInfo, OrderItem } from '../types';
import { OrderDto } from '@/services/orderService';

export interface ICheckoutService {
    validateStock(productCode: string, quantity: number): Promise<boolean>;
    submitOrder(customer: CustomerInfo, items: OrderItem[]): Promise<OrderDto>;
}
