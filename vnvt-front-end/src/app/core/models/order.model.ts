import { OrderItem } from './orderItem.model';
import { Product } from './product.model';

export interface Order {
  id: number;
  orderDate: Date;
  status: string;
  items: OrderItem[];
  totalAmount: number;
}
