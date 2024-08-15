import { Product } from "./product.model";

export interface CartItem extends Product {
  price: number;
  quantity: number;
}
