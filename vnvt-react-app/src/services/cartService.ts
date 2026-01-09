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
function mapCartDtoToCartItems(dto: CartDto): CartItem[] {
    if (!dto || !dto.cartItems) return [];

    return dto.cartItems.map(item => ({
        id: item.code, // Unique ID for cart item
        quantity: item.quantity,
        size: item.size,
        color: item.color,
        product: {
            id: item.productCode,
            name: item.productName,
            price: item.price,
            image: item.productImage || 'https://picsum.photos/seed/product/400/400',
            slug: item.productCode.toLowerCase(), // Missing from DTO, verify if needed
            description: '', // Missing
            category: '', // Missing
            categoryId: '',
            stock: 99, // Unknown from Cart DTO, assumes available
            rating: 0,
            reviewCount: 0,
            createdAt: new Date().toISOString()
        } as Product
    }));
}

// ============ Service ============
export const cartService = {
    async getMyCart(): Promise<ApiResponse<CartDto>> {
        return apiClient.get<CartDto>('/carts/my-cart');
    },

    async addToCart(data: AddToCartRequest): Promise<ApiResponse<CartDto>> {
        return apiClient.post<CartDto>('/carts', data);
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
