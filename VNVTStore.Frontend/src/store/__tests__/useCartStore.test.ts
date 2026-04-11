import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useCartStore, useAuthStore } from '../index';
import { cartService } from '@/services';
import { promotionService } from '@/services/promotionService';
import { Product } from '@/types';

// Mock services
vi.mock('@/services', () => ({
    cartService: {
        getMyCart: vi.fn(),
        addToCart: vi.fn(),
        updateCartItem: vi.fn(),
        removeFromCart: vi.fn(),
        clearCart: vi.fn(),
        mapToFrontend: vi.fn((data) => data),
    }
}));

vi.mock('@/services/promotionService', () => ({
    promotionService: {
        getByCode: vi.fn(),
    }
}));

describe('useCartStore', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        // Reset stores
        useCartStore.setState({ items: [], isLoading: false, coupon: null, discountAmount: 0 });
        useAuthStore.setState({ isAuthenticated: false, user: null, token: null });
    });

    it('fetchCart should load items from service when authenticated', async () => {
        act(() => useAuthStore.setState({ isAuthenticated: true }));

        const mockCartItems = [{ productCode: 'P1', quantity: 2, unitPrice: 100, totalPrice: 200 }];
        (cartService.getMyCart as Mock).mockResolvedValue({ success: true, data: mockCartItems });
        (cartService.mapToFrontend as Mock).mockReturnValue(mockCartItems);

        const { result } = renderHook(() => useCartStore());
        await act(async () => {
            await result.current.fetchCart();
        });

        expect(cartService.getMyCart).toHaveBeenCalled();
        expect(result.current.items).toEqual(mockCartItems);
    });

    it('addItem should update local state when Guest', async () => {
        const mockProduct = { code: 'P1', name: 'Test', price: 100 } as unknown as Product;
        const { result } = renderHook(() => useCartStore());

        await act(async () => {
            await result.current.addItem(mockProduct, 2);
        });

        expect(result.current.items).toHaveLength(1);
        expect(result.current.items[0].quantity).toBe(2);
    });

    it('updateQuantity should call service when authenticated', async () => {
        act(() => useAuthStore.setState({ isAuthenticated: true }));
        (cartService.updateCartItem as Mock).mockResolvedValue({ success: true, data: [] });

        const { result } = renderHook(() => useCartStore());
        await act(async () => {
            await result.current.updateQuantity('ITEM1', 5);
        });

        expect(cartService.updateCartItem).toHaveBeenCalledWith({ itemCode: 'ITEM1', quantity: 5 });
    });

    it('removeItem should work for Guest', async () => {
        act(() => {
            useCartStore.setState({ items: [{ code: 'ITEM1', product: { code: 'P1', price: 100 } as any, quantity: 1 }] });
        });

        const { result } = renderHook(() => useCartStore());
        await act(async () => {
            await result.current.removeItem('ITEM1');
        });

        expect(result.current.items).toHaveLength(0);
    });

    it('applyCoupon should calculate discount correctly (Percentage)', async () => {
        act(() => {
            useCartStore.setState({ items: [{ code: 'ITEM1', product: { code: 'P1', price: 100000 } as any, quantity: 2 }] });
        });

        const mockPromotion = {
            code: 'SALE10',
            discountType: 'PERCENTAGE',
            discountValue: 10,
            minOrderAmount: 100000,
            isActive: true,
            startDate: '2020-01-01',
            endDate: '2099-01-01'
        };
        (promotionService.getByCode as Mock).mockResolvedValue({ success: true, data: mockPromotion });

        const { result } = renderHook(() => useCartStore());
        await act(async () => {
            await result.current.applyCoupon('SALE10');
        });

        expect(result.current.discountAmount).toBe(20000); // 10% of 200k
    });

    it('getTotal should return correct summation', () => {
        useCartStore.setState({ 
            items: [
                { code: '1', product: { price: 100 } as any, quantity: 2 },
                { code: '2', product: { price: 50 } as any, quantity: 3 }
            ] 
        });

        const { result } = renderHook(() => useCartStore());
        expect(result.current.getTotal()).toBe(350); // (100*2) + (50*3)
    });
});
