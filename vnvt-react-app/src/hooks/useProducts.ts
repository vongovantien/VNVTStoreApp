/**
 * React Query hooks for Products
 * Provides data fetching and mutations for product operations
 */

import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';

import { productService, categoryService, type CreateProductDto, type UpdateProductDto } from '@/services/productService';

// ============ Query Keys ============
export const productKeys = {
    all: ['products'] as const,
    lists: () => [...productKeys.all, 'list'] as const,
    list: (params: Record<string, unknown>) => [...productKeys.lists(), params] as const,
    details: () => [...productKeys.all, 'detail'] as const,
    detail: (code: string) => [...productKeys.details(), code] as const,
    categories: ['categories'] as const,
};

// ============ Query Hooks ============

/**
 * Hook for fetching all categories for selection
 */
export function useCategories() {
    return useQuery({
        queryKey: productKeys.categories,
        queryFn: () => categoryService.getAllCategories(),
        select: (response) => {
            if (response.success && response.data) {
                return response.data.items || [];
            }
            return [];
        }

    });
}

/**
 * Hook for fetching products with pagination and search
 */
export function useProducts(params: {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    sortField?: string;
    sortDir?: 'asc' | 'desc';
    enabled?: boolean;
}) {
    const { enabled = true, ...searchParams } = params;

    return useQuery({
        queryKey: productKeys.list(searchParams),
        queryFn: () => productService.searchProducts(searchParams),
        enabled,
        placeholderData: keepPreviousData,
        select: (response) => {

            if (response.success && response.data) {
                return {
                    products: response.data.items,
                    totalItems: response.data.totalItems,
                    totalPages: response.data.totalPages,
                    pageNumber: response.data.pageNumber,
                    pageSize: response.data.pageSize,
                    hasNextPage: response.data.hasNextPage,
                    hasPreviousPage: response.data.hasPreviousPage,
                };
            }
            return {
                products: [],
                totalItems: 0,
                totalPages: 0,
                pageNumber: 1,
                pageSize: 10,
                hasNextPage: false,
                hasPreviousPage: false,
            };
        },
    });
}

/**
 * Hook for fetching single product by code
 */
export function useProduct(code: string, enabled = true) {
    return useQuery({
        queryKey: productKeys.detail(code),
        queryFn: () => productService.getProductByCode(code),
        enabled: enabled && !!code,
        select: (response) => {
            if (response.success && response.data) {
                return response.data;
            }
            return null;
        },
    });
}

// ============ Mutation Hooks ============

/**
 * Hook for creating a new product
 */
export function useCreateProduct() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CreateProductDto) => productService.createProduct(data),
        onSuccess: () => {
            // Invalidate products list to refetch
            queryClient.invalidateQueries({ queryKey: productKeys.lists() });
        },
    });
}

/**
 * Hook for updating a product
 */
export function useUpdateProduct() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ code, data }: { code: string; data: UpdateProductDto }) =>
            productService.updateProduct(code, data),
        onSuccess: (_, variables) => {
            // Invalidate specific product and lists
            queryClient.invalidateQueries({ queryKey: productKeys.detail(variables.code) });
            queryClient.invalidateQueries({ queryKey: productKeys.lists() });
        },
    });
}

/**
 * Hook for deleting a product
 */
export function useDeleteProduct() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (code: string) => productService.deleteProduct(code),
        onSuccess: () => {
            // Invalidate products list
            queryClient.invalidateQueries({ queryKey: productKeys.lists() });
        },
    });
}
