/**
* Product Service
* Uses only baseService CRUD methods
*/

import { createEntityService, API_ENDPOINTS } from './baseService';
import apiClient, { ApiResponse } from './api';

// ============ Types ============
export interface ProductImageDto {
    code: string;
    imageUrl?: string;
    altText?: string;
    sortOrder?: number;
    isPrimary?: boolean;
}

export interface ProductDto {
    code: string;
    name: string;
    description?: string;
    price: number;
    costPrice?: number;
    categoryCode?: string;
    categoryName?: string;
    stockQuantity?: number;

    weight?: number;
    isActive?: boolean;
    color?: string;
    power?: string;
    voltage?: string;
    material?: string;
    size?: string;
    createdAt?: string;
    productImages: ProductImageDto[];
}

export interface CreateProductRequest {
    name: string;
    description?: string;
    price: number;
    costPrice?: number;
    categoryCode?: string;
    stockQuantity?: number;
    stock?: number;

    weight?: number;
    supplierCode?: string;
    brand?: string;
    brandCode?: string;
    color?: string;
    power?: string;
    voltage?: string;
    material?: string;
    size?: string;
    images?: string[];
    isActive?: boolean;
    details?: Array<{
        detailType: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE';
        specName: string;
        specValue: string;
    }>;
    baseUnit?: string;
    minStockLevel?: number;
    binLocation?: string;
    vatRate?: number;
    countryOfOrigin?: string;
    productUnits?: any[];
}

export interface UpdateProductRequest extends Partial<CreateProductRequest> {
    isActive?: boolean;
}

// ============ Category Types ============
export interface CategoryDto {
    code: string;
    name: string;
    description?: string;
    imageUrl?: string;
    parentCode?: string | null;
    isActive?: boolean;
}

export interface CreateCategoryRequest {
    name: string;
    description?: string;
    imageUrl?: string;
    parentCode?: string | null;
    isActive?: boolean;
}

export interface UpdateCategoryRequest extends Partial<CreateCategoryRequest> {
    isActive?: boolean;
}

export interface ProductStats {
    total: number;
    outOfStock: number;
    lowStock: number;
}

export interface CategoryStats {
    total: number;
    main: number; // Backend returns "main" (Main Categories count)
    active: number;
}

// ============ Services ============
export const productService = {
    ...createEntityService<ProductDto, CreateProductRequest, UpdateProductRequest>({
        endpoint: API_ENDPOINTS.PRODUCTS.BASE,
        resourceName: 'Product'
    }),
    import: async (file: File) => {
        const formData = new FormData();
        formData.append('file', file);
        // Note: Using explicit endpoint or base endpoint + /import
        // Base endpoint is /products. Import is /products/import
        const response = await apiClient.post<ApiResponse<number>>(`${API_ENDPOINTS.PRODUCTS.BASE}/import`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },
    getStats: async (): Promise<ProductStats> => {
        const response = await apiClient.get<ProductStats>(`${API_ENDPOINTS.PRODUCTS.BASE}/stats`);
        // Fallback if backend doesn't implement stats
        if (!response.success && response.message?.includes('404')) {
            return {
                total: 0,
                outOfStock: 0,
                lowStock: 0
            }
        }
        return response.data || { total: 0, outOfStock: 0, lowStock: 0 };
    }
};

export const categoryService = {
    ...createEntityService<CategoryDto, CreateCategoryRequest, UpdateCategoryRequest>({
        endpoint: API_ENDPOINTS.CATEGORIES.BASE,
        resourceName: 'Category'
    }),
    getStats: async (): Promise<CategoryStats> => {
        const response = await apiClient.get<CategoryStats>(`${API_ENDPOINTS.CATEGORIES.BASE}/stats`);
        if (!response.success && response.message?.includes('404')) {
            return { total: 0, main: 0, active: 0 }
        }
        return response.data || { total: 0, main: 0, active: 0 };
    }
};

export default productService;
