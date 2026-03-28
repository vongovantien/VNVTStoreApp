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

    return dto.cartItems.map(item => {
        // Use property checking instead of any
        const productName = item.productName || 'Unknown Product';
        const productPrice = item.price || 0;
        const productImage = item.productImage || 'https://picsum.photos/seed/product/400/400';

        return {
            code: item.code,
            quantity: item.quantity,
            size: item.size,
            color: item.color,
            product: {
                code: item.productCode,
                name: productName,
                price: productPrice,
                image: productImage,
                slug: (item.productCode || '').toLowerCase(),
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

    async addMultipleToCart(items: AddToCartRequest[]): Promise<ApiResponse<CartDto>> {
        return apiClient.post<CartDto>('/carts/items/bulk', { items });
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
