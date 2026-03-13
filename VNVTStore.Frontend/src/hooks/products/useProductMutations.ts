import { useMutation, useQueryClient } from '@tanstack/react-query';
import { productService, type CreateProductRequest, type UpdateProductRequest } from '@/services/productService';
import { productKeys } from './keys';

// ============ Mutation Hooks ============

/**
 * Hook for creating a new product
 */
export function useCreateProduct() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CreateProductRequest) => productService.create(data),
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
        mutationFn: ({ code, data }: { code: string; data: UpdateProductRequest }) =>
            productService.update(code, data),
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
        mutationFn: (code: string) => productService.delete(code),
        onSuccess: () => {
            // Invalidate products list
            queryClient.invalidateQueries({ queryKey: productKeys.lists() });
        },
    });
}
