import { renderHook, waitFor } from '@testing-library/react';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { useProducts, useProduct } from '../useProducts';
import { productService } from '@/services/productService';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';

// Mock productService
vi.mock('@/services/productService', () => ({
    productService: {
        search: vi.fn(),
        getByCode: vi.fn(),
    },
    categoryService: {
        search: vi.fn(),
    }
}));

// Create a wrapper for QueryClientProvider
const createWrapper = () => {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: {
                retry: false,
            },
        },
    });
    const Wrapper = ({ children }: { children: React.ReactNode }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
    Wrapper.displayName = 'QueryClientWrapper';
    return Wrapper;
};

describe('useProducts hook', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('fetches products and transforms data correctly', async () => {
        const mockResponse = {
            success: true,
            data: {
                items: [
                    {
                        code: 'PROD1',
                        name: 'Product 1',
                        price: 100000,
                        stockQuantity: 10,
                        categoryName: 'Cat 1',
                        productImages: [{ imageURL: 'img1.jpg', isPrimary: true }]
                    }
                ],
                totalItems: 1
            }
        };

        vi.mocked(productService.search).mockResolvedValue(mockResponse);

        const { result } = renderHook(() => useProducts({ pageIndex: 1, pageSize: 10 }), {
            wrapper: createWrapper()
        });

        await waitFor(() => expect(result.current.isSuccess).toBe(true));

        expect(result.current.data?.products).toHaveLength(1);
        expect(result.current.data?.products[0].name).toBe('Product 1');
        expect(result.current.data?.products[0].stock).toBe(10);
    });

    it('handles empty response correctly', async () => {
        vi.mocked(productService.search).mockResolvedValue({
            success: false,
            message: 'Error'
        });

        const { result } = renderHook(() => useProducts({ pageIndex: 1, pageSize: 10 }), {
            wrapper: createWrapper()
        });

        await waitFor(() => expect(result.current.isSuccess).toBe(true));

        expect(result.current.data?.products).toHaveLength(0);
        expect(result.current.data?.totalItems).toBe(0);
    });
});

describe('useProduct hook', () => {
    it('fetches a single product details', async () => {
        const mockProduct = {
            code: 'PROD1',
            name: 'Product 1',
            price: 100000,
            stockQuantity: 10,
            productImages: [{ imageURL: 'img1.jpg', isPrimary: true }]
        };

        vi.mocked(productService.getByCode).mockResolvedValue({
            success: true,
            data: mockProduct
        });

        const { result } = renderHook(() => useProduct('PROD1'), {
            wrapper: createWrapper()
        });

        await waitFor(() => expect(result.current.isSuccess).toBe(true));

        expect(result.current.data?.name).toBe('Product 1');
        expect(result.current.data?.code).toBe('PROD1');
    });
});
