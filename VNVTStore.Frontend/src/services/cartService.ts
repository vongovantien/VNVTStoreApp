import { apiClient, type ApiResponse } from './api';
import type { CartItem, Product } from '@/types';

// ============ API DTOs ============
export interface CartItemDto {
    code: string;
    productCode: string;
    productName: string;
    productImage: string;
    price: number;
    quantity: number;
    size?: string;
    color?: string;
}

export interface CartDto {
    code: string;
    userCode: string;
    cartItems: CartItemDto[];
    totalAmount: number;
}

export interface AddToCartRequest {
    productCode: string;
    quantity: number;
    size?: string;
    color?: string;
}

export interface UpdateCartItemRequest {
    itemCode: string;
    quantity: number;
}

// ============ Mapper ============
// ============ Mapper ============
function mapCartDtoToCartItems(dto: CartDto): CartItem[] {
    if (!dto || !dto.cartItems) return [];

    return dto.cartItems.map(item => {
        // Handle potential PascalCase or different naming from backend
        const rawItem = item as any;
        const productName = item.productName || rawItem.ProductName || 'Unknown Product';
        const productPrice = item.price || rawItem.ProductPrice || rawItem.Price || 0;
        const productImage = item.productImage || rawItem.ProductImage || 'https://picsum.photos/seed/product/400/400';

        return {
            code: item.code || rawItem.Code,
            quantity: item.quantity || rawItem.Quantity,
            size: item.size || rawItem.Size,
            color: item.color || rawItem.Color,
            product: {
                code: item.productCode || rawItem.ProductCode,
                name: productName,
                price: productPrice,
                image: productImage,
                slug: (item.productCode || rawItem.ProductCode || '').toLowerCase(),
                description: '',
                category: '',
                categoryId: '',
                stock: 99,
                rating: 0,
                reviewCount: 0,
                createdAt: new Date().toISOString()
            } as Product
        };
    });
}

// ============ Service ============
export const cartService = {
    async getMyCart(): Promise<ApiResponse<CartDto>> {
        return apiClient.get<CartDto>('/carts');
    },

    async addToCart(data: AddToCartRequest): Promise<ApiResponse<CartDto>> {
        console.log('Sending AddToCart request to /carts/items', data);
        return apiClient.post<CartDto>('/carts/items', data);
    },

    async updateCartItem(data: UpdateCartItemRequest): Promise<ApiResponse<CartDto>> {
        return apiClient.put<CartDto>(`/carts/items/${data.itemCode}`, { quantity: data.quantity });
    },

    async removeFromCart(itemCode: string): Promise<ApiResponse<CartDto>> {
        return apiClient.delete<CartDto>(`/carts/items/${itemCode}`);
    },

    async clearCart(): Promise<ApiResponse<boolean>> {
        return apiClient.delete<boolean>('/carts');
    },

    // Helper to sync with Store
    mapToFrontend: mapCartDtoToCartItems
};

export default cartService;
