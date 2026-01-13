import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { cartService } from '../cartService';
import { apiClient } from '../api';

// Mock API Client module completely
vi.mock('../api', () => {
    const mockFn = {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        delete: vi.fn(),
    };
    return {
        apiClient: mockFn,
        default: mockFn,
    };
});

describe('cartService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const mockGet = apiClient.get as Mock;
    const mockPost = apiClient.post as Mock;
    const mockPut = apiClient.put as Mock;
    const mockDelete = apiClient.delete as Mock;

    describe('getMyCart', () => {
        it('should call apiClient.get with correct endpoint', async () => {
            const mockResponse = { success: true, data: [] };
            mockGet.mockResolvedValue(mockResponse);

            const result = await cartService.getMyCart();

            expect(mockGet).toHaveBeenCalledWith('/carts/my-cart');
            expect(result).toBe(mockResponse);
        });
    });

    describe('addToCart', () => {
        it('should call apiClient.post with correct endpoint for Add', async () => {
            const item = { productCode: 'P1', quantity: 1 };
            const mockResponse = { success: true, data: true };
            mockPost.mockResolvedValue(mockResponse);

            const result = await cartService.addToCart(item);

            expect(mockPost).toHaveBeenCalledWith('/carts', item);
            expect(result).toBe(mockResponse);
        });
    });

    describe('updateCartItem', () => {
        it('should call apiClient.put with correct endpoint', async () => {
            const update = { itemCode: 'I1', quantity: 2 };
            // Service sends only quantity in body
            const mockResponse = { success: true, data: true };
            mockPut.mockResolvedValue(mockResponse);

            const result = await cartService.updateCartItem(update);

            expect(mockPut).toHaveBeenCalledWith('/carts/items/I1', { quantity: 2 });
            expect(result).toBe(mockResponse);
        });
    });

    describe('removeFromCart', () => {
        it('should call apiClient.delete with correct endpoint', async () => {
            const mockResponse = { success: true, data: true };
            mockDelete.mockResolvedValue(mockResponse);

            const result = await cartService.removeFromCart('P1');

            expect(mockDelete).toHaveBeenCalledWith('/carts/items/P1');
            expect(result).toBe(mockResponse);
        });
    });

    describe('clearCart', () => {
        it('should call apiClient.delete on /carts', async () => {
            const mockResponse = { success: true, data: true };
            mockDelete.mockResolvedValue(mockResponse);

            const result = await cartService.clearCart();

            expect(mockDelete).toHaveBeenCalledWith('/carts');
            expect(result).toBe(mockResponse);
        });
    });
});
