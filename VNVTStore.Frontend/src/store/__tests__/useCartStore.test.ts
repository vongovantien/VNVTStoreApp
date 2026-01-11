import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useCartStore, useAuthStore } from '../index';
import { cartService } from '@/services';

// Mock cartService
vi.mock('@/services', () => ({
    cartService: {
        getMyCart: vi.fn(),
        addToCart: vi.fn(),
        updateCartItem: vi.fn(),
        removeFromCart: vi.fn(),
        clearCart: vi.fn(),
        mapToFrontend: vi.fn((data) => data), // Simple identity mock or specific logic
    }
}));

describe('useCartStore', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        // Reset stores
        useCartStore.setState({ items: [], totalAmount: 0, isLoading: false, error: null } as any);
        useAuthStore.setState({ isAuthenticated: false, user: null, token: null });
    });

    it('fetchCart should load items from service when authenticated', async () => {
        // Authenticate user
        act(() => useAuthStore.setState({ isAuthenticated: true }));

        const mockCartItems = [{ productCode: 'P1', quantity: 2, unitPrice: 100, totalPrice: 200 }];
        const mockResponse = { success: true, data: mockCartItems };
        (cartService.getMyCart as any).mockResolvedValue(mockResponse);
        (cartService.mapToFrontend as any).mockReturnValue(mockCartItems);

        const { result } = renderHook(() => useCartStore());

        await act(async () => {
            await result.current.fetchCart();
        });

        expect(cartService.getMyCart).toHaveBeenCalled();
        expect(result.current.items).toEqual(mockCartItems);
        expect(result.current.isLoading).toBe(false);
    });

    it('fetchCart should NOT load items if not authenticated', async () => {
        // useAuthStore isAuthenticated is false by default in beforeEach

        const { result } = renderHook(() => useCartStore());

        await act(async () => {
            await result.current.fetchCart();
        });

        expect(cartService.getMyCart).not.toHaveBeenCalled();
    });

    it('addItem should call service when authenticated', async () => {
        act(() => useAuthStore.setState({ isAuthenticated: true }));

        const mockProduct = { id: 'P1', name: 'Test', price: 100, image: 'img.jpg' } as any;
        const mockResponse = { success: true, data: [] };
        (cartService.addToCart as any).mockResolvedValue(mockResponse);
        (cartService.getMyCart as any).mockResolvedValue({ success: true, data: [] });
        (cartService.mapToFrontend as any).mockReturnValue([]);

        const { result } = renderHook(() => useCartStore());

        await act(async () => {
            await result.current.addItem(mockProduct, 1);
        });

        expect(cartService.addToCart).toHaveBeenCalledWith(expect.objectContaining({ productCode: 'P1', quantity: 1 }));
    });

    it('addItem should update local state when NOT authenticated (Guest)', async () => {
        // isAuthenticated false
        const mockProduct = { id: 'P1', name: 'Test', price: 100, image: 'img.jpg' } as any;

        const { result } = renderHook(() => useCartStore());

        await act(async () => {
            await result.current.addItem(mockProduct, 2);
        });

        expect(cartService.addToCart).not.toHaveBeenCalled();
        expect(result.current.items).toHaveLength(1);
        expect(result.current.items[0].quantity).toBe(2);
        expect(result.current.items[0].product.id).toBe('P1');
    });
});
