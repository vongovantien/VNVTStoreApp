/**
 * React Query hooks for Products
 * Provides data fetching and mutations for product operations
 */

import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';

import { Product, ProductDetail, ProductUnit, ProductVariant } from '@/types';
import { productService, categoryService, type CreateProductRequest, type UpdateProductRequest, type ProductDto, type ProductImageDto } from '@/services/productService';
import { SearchCondition } from '@/services/baseService';
import { getImageUrl } from '@/utils/format';
import { CATEGORY_LIST_FIELDS, PRODUCT_LIST_FIELDS, PRODUCT_DETAIL_FIELDS } from '@/constants/fieldConstants';

// ============ Query Keys ============
export const productKeys = {
    all: ['products'] as const,
    lists: () => [...productKeys.all, 'list'] as const,
    list: (params: Record<string, unknown>) => [...productKeys.lists(), params] as const,
    details: () => [...productKeys.all, 'detail'] as const,
    detail: (code: string) => [...productKeys.details(), code] as const,
    stats: () => [...productKeys.all, 'stats'] as const,
    categories: ['categories'] as const,
};

// ============ Helper Functions ============

/**
 * Extract product images from DTO (handles multiple casing variations)
 */
function getProductImages(item: ProductDto | Record<string, unknown>): ProductImageDto[] | undefined {
    const dto = item as ProductDto;
    const record = item as Record<string, unknown>;
    return (dto.productImages || record.ProductImages || record.product_images) as ProductImageDto[] | undefined;
}

/**
 * Maps ProductDto to Frontend Product model
 * Centralized mapping to avoid duplication (DRY principle)
 */
function mapProductDtoToProduct(item: ProductDto, options?: { includeDetails?: boolean }): Product {
    const images = getProductImages(item);
    const primaryImg = images?.find((img: ProductImageDto) => img.isPrimary) || images?.[0];

    const baseProduct: Product = {
        code: item.code,
        name: item.name,
        slug: item.code,
        description: item.description || '',
        price: item.price,
        image: getImageUrl(primaryImg?.imageURL),
        images: images?.map((img: ProductImageDto) => getImageUrl(img.imageURL)) || [],
        productImages: images || [],
        category: item.categoryName || '',
        categoryCode: item.categoryCode || '',
        stock: item.stockQuantity || 0,
        // Attributes
        color: item.color,
        power: item.power,
        voltage: item.voltage,
        material: item.material,
        size: item.size,
        // Default values
        brand: item.brand || 'VNVT',
        rating: 5,
        reviewCount: 0,
        isFeatured: item.isFeatured || false,
        isNew: item.isNew ?? true,
        createdAt: item.createdAt || new Date().toISOString()
    };

    // Add detail-specific fields when fetching single product
    if (options?.includeDetails) {
        return {
            ...baseProduct,
            wholesalePrice: item.wholesalePrice,
            costPrice: item.costPrice,
            stockQuantity: item.stockQuantity,
            countryOfOrigin: item.countryOfOrigin,
            baseUnit: item.baseUnit,
            binLocation: item.binLocation,
            details: (item.details as unknown as ProductDetail[]) || [],
            productUnits: (item.productUnits as unknown as ProductUnit[]) || [],
            variants: (item.variants as unknown as ProductVariant[]) || [],
            originalPrice: item.price * 1.2,
        };
    }

    return baseProduct;
}

// ============ Query Hooks ============

/**
 * Hook for fetching all categories for selection
 */
export function useCategories(params?: {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    enabled?: boolean;
    fields?: string[];
}) {
    const {
        enabled,
        pageIndex = 1,
        pageSize = 1000,
        search,
        fields = CATEGORY_LIST_FIELDS
    } = params || {};

    return useQuery({
        queryKey: [...productKeys.categories, { pageIndex, pageSize, search, fields }],
        queryFn: () => categoryService.search({
            pageIndex,
            pageSize,
            search,
            filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
            searchField: 'name',
            fields
        }),
        enabled: enabled,
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
    fields?: string[];  // Selective columns to fetch
}) {
    const { fields = CATEGORY_LIST_FIELDS, ...searchParams } = params;

    // Build filters
    const filters: { field: string; value: string; operator?: SearchCondition }[] = [];
    if (searchParams.parentCode) filters.push({ field: 'parentCode', value: searchParams.parentCode });
    if (searchParams.isActive) filters.push({ field: 'isActive', value: searchParams.isActive, operator: SearchCondition.Equal });
    if (searchParams.search) filters.push({ field: 'name', value: searchParams.search, operator: SearchCondition.Contains });


    return useQuery({
        queryKey: [...productKeys.categories, { ...searchParams, fields }],
        queryFn: () => categoryService.search({
            pageIndex: searchParams.pageIndex,
            pageSize: searchParams.pageSize,
            // Search is handled via filters for precise control or generic search
            // If we use baseService search, we can pass filters directly
            filters: filters.length > 0 ? filters : undefined,
            fields,  // Pass fields for selective column fetching
        }),
        placeholderData: keepPreviousData,
        select: (response) => {
            if (response.success && response.data) {
                const pageSize = searchParams.pageSize || 10;
                const pageIndex = searchParams.pageIndex || 1;
                const totalItems = response.data.totalItems;
                const totalPages = Math.ceil(totalItems / pageSize);

                // Deduplicate
                const uniqueItems = Array.from(new Map((response.data.items || []).map(item => [item.code, item])).values());

                return {
                    categories: uniqueItems,
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
    brands?: string[];
    minPrice?: number;
    maxPrice?: number;
    rating?: number;
    priceType?: 'all' | 'fixed' | 'contact';
    enabled?: boolean;
    ids?: string[];
    fields?: string[];  // Selective columns to fetch (reduces data transfer)
}) {
    const { enabled = true, fields = PRODUCT_LIST_FIELDS, ...searchParams } = params;

    // Build filters dynamically
    const filters: { field: string; value: string | string[] | number | number[] | boolean; operator?: SearchCondition }[] = [];

    // 1. Generic/Dynamic params (excluding specific ones)
    Object.entries(searchParams).forEach(([key, value]) => {
        if (['pageIndex', 'pageSize', 'search', 'sortField', 'sortDir', 'brands', 'minPrice', 'maxPrice', 'rating', 'category', 'ids', 'priceType'].includes(key)) return;
        if (value !== undefined && value !== null && value !== '') {
            filters.push({ field: key, value: String(value), operator: SearchCondition.Equal });
        }
    });

    // 0. IDs (Multi-select)
    if (searchParams.ids && searchParams.ids.length > 0) {
        filters.push({
            field: 'Code',
            value: searchParams.ids,
            operator: SearchCondition.In
        });
    }

    // 2. Category (Multi-select)
    if (searchParams.category) {
        if (searchParams.category.includes(',')) {
            const cats = searchParams.category.split(',').map(c => c.trim());
            filters.push({
                field: 'CategoryCode',
                value: cats,
                operator: SearchCondition.In
            });
        } else {
            filters.push({
                field: 'CategoryCode',
                value: searchParams.category,
                operator: SearchCondition.Equal
            });
        }
    }

    // 2. Brands (Multi-select) -> In Operator
    if (searchParams.brands && searchParams.brands.length > 0) {
        filters.push({
            field: 'brand', // Make sure backend entity has 'Brand' property
            value: searchParams.brands,
            operator: SearchCondition.In
        });
    }

    // 3. Price Range
    if (searchParams.minPrice !== undefined) {
        filters.push({ field: 'price', value: searchParams.minPrice, operator: SearchCondition.GreaterThanEqual });
    }
    if (searchParams.maxPrice !== undefined) {
        filters.push({ field: 'price', value: searchParams.maxPrice, operator: SearchCondition.LessThanEqual });
    }

    // 4a. Price Type (Fixed vs Contact)
    if (searchParams.priceType) {
        if (searchParams.priceType === 'contact') {
            filters.push({ field: 'price', value: 0, operator: SearchCondition.Equal });
        } else if (searchParams.priceType === 'fixed') {
            filters.push({ field: 'price', value: 0, operator: SearchCondition.GreaterThan });
        }
    }

    // 4. Rating
    if (searchParams.rating !== undefined && searchParams.rating !== null) {
        filters.push({ field: 'rating', value: searchParams.rating, operator: SearchCondition.GreaterThanEqual });
    }


    return useQuery({
        queryKey: productKeys.list({ ...searchParams, fields }),
        queryFn: () => productService.search({
            pageIndex: searchParams.pageIndex,
            pageSize: searchParams.pageSize,
            search: searchParams.search,
            searchField: 'name',
            sortBy: searchParams.sortField,
            sortDesc: searchParams.sortDir === 'desc',
            filters: filters.length > 0 ? filters : undefined,
            fields,  // Pass fields for selective column fetching
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

                // Deduplicate items based on code to prevent UI duplication issues
                const uniqueItems = Array.from(new Map((response.data.items || []).map(item => [item.code, item])).values());

                // Map ProductDto to Frontend Product model using helper
                const products: Product[] = uniqueItems.map((rawItem) =>
                    mapProductDtoToProduct(rawItem as ProductDto)
                );

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
        queryFn: () => productService.getByCode(code, { includeChildren: true }),
        enabled: !!code,
        select: (response): Product | null => {
            if (response.success && response.data) {
                // Map ProductDto to Frontend Product model using helper with details
                return mapProductDtoToProduct(response.data as ProductDto, { includeDetails: true });
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
