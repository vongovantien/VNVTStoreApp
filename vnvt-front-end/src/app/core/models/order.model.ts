import { OrderItem } from './orderItem.model';
import { Payment } from './payment.model';
import { Product } from './product.model';

export interface Order {
  id: number;
  userId: number;
  orderStatus: string;
  totalAmount: number;
  items: OrderItem[];
  payment: Payment;
}
