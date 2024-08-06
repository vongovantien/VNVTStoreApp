export interface Product {
  id: number;
  name: string;
  createdDate: string;
  updatedDate: string;
  categoryName: string;
  description: string;
  price: number;
  categoryId: number;
  stockQuantity: number;
  productImages: ProductImage[];
  imageUrl: string;
}

export interface ProductImage {
  id: number;
  createdDate: string;
  updatedDate: string;
  imageUrl: string;
  productId: number;
}
