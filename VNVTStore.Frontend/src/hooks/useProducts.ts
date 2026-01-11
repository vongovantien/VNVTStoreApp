/**
 * React Query hooks for Products
 * Provides data fetching and mutations for product operations
 */

import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';

import { Product } from '@/types';
import { productService, categoryService, type CreateProductRequest, type UpdateProductRequest } from '@/services/productService';
import { SearchCondition } from '@/services/baseService';

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
 * Hook for fetching categories with pagination
 */
export function useCategoriesList(params: {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    parentCode?: string;
    isActive?: string; // 'true' | 'false'
}) {
    const { ...searchParams } = params;

    // Build filters
    const filters: { field: string; value: string; operator?: SearchCondition }[] = [];
    if (searchParams.parentCode) filters.push({ field: 'parentCode', value: searchParams.parentCode });
    if (searchParams.isActive) filters.push({ field: 'isActive', value: searchParams.isActive });
    if (searchParams.search) filters.push({ field: 'name', value: searchParams.search, operator: SearchCondition.Contains });


    return useQuery({
        queryKey: [...productKeys.categories, searchParams],
        queryFn: () => categoryService.search({
            pageIndex: searchParams.pageIndex,
            pageSize: searchParams.pageSize,
            // Search is handled via filters for precise control or generic search
            // If we use baseService search, we can pass filters directly
            filters: filters.length > 0 ? filters : undefined
        }),
        placeholderData: keepPreviousData,
        select: (response) => {
            if (response.success && response.data) {
                const pageSize = searchParams.pageSize || 10;
                const pageIndex = searchParams.pageIndex || 1;
                const totalItems = response.data.totalItems;
                const totalPages = Math.ceil(totalItems / pageSize);

                return {
                    categories: response.data.items || [],
                    totalItems,
                    totalPages,
                    pageNumber: pageIndex,
                    pageSize,
                };
            }
            return {
                categories: [],
                totalItems: 0,
                totalPages: 0,
                pageNumber: 1,
                pageSize: 10,
            };
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

    // Build filters dynamically
    const filters = Object.entries(searchParams).reduce((acc, [key, value]) => {
        // Skip pagination/sorting params
        if (['pageIndex', 'pageSize', 'search', 'sortField', 'sortDir'].includes(key)) return acc;
        if (value !== undefined && value !== null && value !== '') {
            acc.push({ field: key, value: String(value), operator: SearchCondition.Equal });
        }
        return acc;
    }, [] as Array<{ field: string; value: string; operator?: SearchCondition }>);

    return useQuery({
        queryKey: productKeys.list(searchParams),
        queryFn: () => productService.search({
            pageIndex: searchParams.pageIndex,
            pageSize: searchParams.pageSize,
            search: searchParams.search,
            searchField: 'name',
            sortBy: searchParams.sortField,
            sortDesc: searchParams.sortDir === 'desc',
            filters: filters.length > 0 ? filters : undefined
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
                const pageSize = searchParams.pageSize || 10;
                const pageIndex = searchParams.pageIndex || 1;
                const totalItems = response.data.totalItems;
                const totalPages = Math.ceil(totalItems / pageSize);

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
                    // Attributes
                    color: item.color,
                    power: item.power,
                    voltage: item.voltage,
                    material: item.material,
                    size: item.size,
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
                    totalItems,
                    totalPages,
                    pageNumber: pageIndex,
                    pageSize,
                    hasNextPage: pageIndex < totalPages,
                    hasPreviousPage: pageIndex > 1,
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
