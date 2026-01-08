/**
 * Product Service
 * Handles all product-related API calls
 */

import { apiClient, type ApiResponse, type PagedResult, type RequestDTO } from './api';
import type { Product } from '@/types';

// ============ API Response Types ============
export interface ProductApiDto {
    code: string;
    name: string;
    description?: string;
    price: number;
    costPrice?: number;
    categoryCode?: string;
    categoryName?: string;
    stockQuantity?: number;
    sku?: string;
    weight?: number;
    isActive?: boolean;
    createdAt?: string;
    productImages: ProductImageDto[];
}

export interface ProductImageDto {
    code: string;
    imageUrl?: string;
    altText?: string;
    sortOrder?: number;
    isPrimary?: boolean;
}

export interface CreateProductDto {
    name: string;
    description?: string;
    price: number;
    costPrice?: number;
    categoryCode?: string;
    stockQuantity?: number;
    sku?: string;
    weight?: number;
}

export interface UpdateProductDto {
    name?: string;
    description?: string;
    price?: number;
    costPrice?: number;
    categoryCode?: string;
    stockQuantity?: number;
    sku?: string;
    weight?: number;
    isActive?: boolean;
}

// ============ Mapper Functions ============
export function mapProductDtoToProduct(dto: ProductApiDto): Product {
    const primaryImage = dto.productImages?.find((img) => img.isPrimary) || dto.productImages?.[0];

    return {
        id: dto.code,
        name: dto.name,
        slug: dto.code.toLowerCase(),
        description: dto.description || '',
        price: dto.price,
        image: primaryImage?.imageUrl || 'https://picsum.photos/seed/product/400/400',
        images: dto.productImages?.map((img) => img.imageUrl || '') || [],
        category: dto.categoryName || 'Chưa phân loại',
        categoryId: dto.categoryCode || '',
        stock: dto.stockQuantity || 0,
        rating: 4.5, // Default rating until reviews are implemented
        reviewCount: 0,
        createdAt: dto.createdAt || new Date().toISOString(),
    };
}

// ============ Product Service ============
export const productService = {
    /**
     * Search products with pagination and filters
     */
    async searchProducts(params: {
        pageIndex?: number;
        pageSize?: number;
        search?: string;
        sortField?: string;
        sortDir?: 'asc' | 'desc';
    }): Promise<ApiResponse<PagedResult<Product>>> {
        const request: RequestDTO = {
            pageIndex: params.pageIndex || 1,
            pageSize: params.pageSize || 10,
            searching: params.search
                ? [{ field: 'name', operator: 'contains', value: params.search }]
                : undefined,
            sortDTO: params.sortField
                ? { sortBy: params.sortField, sortDescending: params.sortDir === 'desc' }
                : undefined,
        };

        const response = await apiClient.post<PagedResult<ProductApiDto>>(
            '/products/search',
            request
        );

        if (response.success && response.data) {
            return {
                ...response,
                data: {
                    ...response.data,
                    items: response.data.items.map(mapProductDtoToProduct),
                },
            };
        }

        return {
            ...response,
            data: null,
        };
    },

    /**
     * Get product by code
     */
    async getProductByCode(code: string): Promise<ApiResponse<Product>> {
        const response = await apiClient.get<ProductApiDto>(`/products/${code}`);

        if (response.success && response.data) {
            return {
                ...response,
                data: mapProductDtoToProduct(response.data),
            };
        }

        return {
            ...response,
            data: null,
        };
    },

    /**
     * Create new product (Admin only)
     */
    async createProduct(data: CreateProductDto): Promise<ApiResponse<Product>> {
        const request: RequestDTO<CreateProductDto> = {
            postObject: data,
        };

        const response = await apiClient.post<ProductApiDto>('/products', request);

        if (response.success && response.data) {
            return {
                ...response,
                data: mapProductDtoToProduct(response.data),
            };
        }

        return {
            ...response,
            data: null,
        };
    },

    /**
     * Update product (Admin only)
     */
    async updateProduct(
        code: string,
        data: UpdateProductDto
    ): Promise<ApiResponse<Product>> {
        const request: RequestDTO<UpdateProductDto> = {
            postObject: data,
        };

        const response = await apiClient.put<ProductApiDto>(
            `/products/${code}`,
            request
        );

        if (response.success && response.data) {
            return {
                ...response,
                data: mapProductDtoToProduct(response.data),
            };
        }

        return {
            ...response,
            data: null,
        };
    },

    /**
     * Delete product (Admin only)
     */
    async deleteProduct(code: string): Promise<ApiResponse<void>> {
        return apiClient.delete<void>(`/products/${code}`);
    },
};

export interface CategoryDto {
    code: string;
    name: string;
    description?: string;
    imageUrl?: string;
}


export const categoryService = {
    getAllCategories: async (): Promise<ApiResponse<PagedResult<CategoryDto>>> => {
        const response = await apiClient.post<PagedResult<CategoryDto>>('/categories/search', {
            pageIndex: 1,
            pageSize: 100
        });

        // Map search result to simple array if needed, but assuming API matches
        return response;
    }
};

export default productService;
