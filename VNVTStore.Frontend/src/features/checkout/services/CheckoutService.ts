import { ICheckoutService } from './ICheckoutService';
import { CustomerInfo, OrderItem } from '../types';
import { productService } from '@/services/productService';
import { ICheckoutRepository } from '../repositories/CheckoutRepository';
import { OrderDto } from '@/services/orderService';

export class CheckoutService implements ICheckoutService {
    constructor(private repository: ICheckoutRepository) { }

    async validateStock(productCode: string, quantity: number): Promise<boolean> {
        try {
            const response = await productService.getByCode(productCode);
            if (response.success && response.data) {
                const stockCount = response.data.stockQuantity ?? 0;
                return stockCount >= quantity;
            }
            return false;
        } catch (error) {
            console.error('Stock validation failed:', error);
            return false;
        }
    }

    async submitOrder(customer: CustomerInfo, items: OrderItem[]): Promise<OrderDto> {
        return this.repository.submitOrder(customer, items);
    }
}
