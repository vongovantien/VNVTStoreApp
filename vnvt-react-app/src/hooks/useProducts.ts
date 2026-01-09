/**
 * React Query hooks for Products
 * Provides data fetching and mutations for product operations
 */

import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';

import { Product } from '@/types';
import { productService, categoryService, type CreateProductRequest, type UpdateProductRequest } from '@/services/productService';

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
        queryFn: () => categoryService.getAll(),
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
    category?: string;
    enabled?: boolean;
}) {
    const { enabled = true, ...searchParams } = params;

    return useQuery({
        queryKey: productKeys.list(searchParams),
        queryFn: () => productService.search({
            pageIndex: searchParams.pageIndex,
            pageSize: searchParams.pageSize,
            search: searchParams.search,
            searchField: 'name',
            sortBy: searchParams.sortField,
            sortDesc: searchParams.sortDir === 'desc',
            filters: searchParams.category ? [{ field: 'category', value: searchParams.category }] : undefined
        }),
        enabled,
        placeholderData: keepPreviousData,
        select: (response): {
            products: Product[];
            totalItems: number;
            totalPages: number;
            pageNumber: number;
            pageSize: number;
            hasNextPage: boolean;
            hasPreviousPage: boolean;
        } => {
            if (response.success && response.data) {
                // Map ProductDto to Frontend Product model
                const products: Product[] = (response.data.items || []).map(item => ({
                    id: item.code,
                    name: item.name,
                    slug: item.code, // Use code as slug for now
                    description: item.description || '',
                    price: item.price,
                    image: item.productImages?.find(img => img.isPrimary)?.imageUrl || item.productImages?.[0]?.imageUrl || '',
                    images: item.productImages?.map(img => img.imageUrl || '') || [],
                    category: item.categoryName || '',
                    categoryId: item.categoryCode || '',
                    stock: item.stockQuantity || 0,
                    // Mock/Default values for missing backend fields
                    brand: 'VNVT',
                    rating: 5,
                    reviewCount: 0,
                    isFeatured: false,
                    isNew: true,
                    createdAt: item.createdAt || new Date().toISOString()
                }));

                return {
                    products,
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
 * Hook for fetching single product details
 */
export function useProduct(code: string) {
    return useQuery({
        queryKey: productKeys.detail(code),
        queryFn: () => productService.getByCode(code),
        enabled: !!code,
        select: (response): Product | null => {
            if (response.success && response.data) {
                const item = response.data;
                // Map ProductDto to Frontend Product model
                return {
                    id: item.code,
                    name: item.name,
                    slug: item.code,
                    description: item.description || '',
                    price: item.price,
                    image: item.productImages?.find(img => img.isPrimary)?.imageUrl || item.productImages?.[0]?.imageUrl || '',
                    images: item.productImages?.map(img => img.imageUrl || '') || [],
                    category: item.categoryName || '',
                    categoryId: item.categoryCode || '',
                    stock: item.stockQuantity || 0,
                    // Mock/Default values
                    brand: 'VNVT',
                    rating: 5,
                    reviewCount: 0,
                    isFeatured: false,
                    isNew: true,
                    originalPrice: item.price * 1.2, // Fake original price for demo
                    specifications: {
                        'Thương hiệu': 'VNVT',
                        'Xuất xứ': 'Việt Nam',
                        'Bảo hành': '12 tháng'
                    },
                    createdAt: item.createdAt || new Date().toISOString()
                };
            }
            return null;
        }
    });
}

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
