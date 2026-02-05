/**
* Product Service
* Uses only baseService CRUD methods
*/

import { createEntityService, API_ENDPOINTS } from './baseService';
import apiClient from './api';

// ============ Types ============
export interface ProductImageDto {
    code: string;
    imageURL?: string;
    altText?: string;
    sortOrder?: number;
    isPrimary?: boolean;
}

export interface ProductUnitDto {
    code: string;
    productCode: string;
    unitCode?: string;
    unitName: string;
    conversionRate: number;
    price: number;
    isBaseUnit: boolean;
    isActive?: boolean;
}

export interface ProductDetailDto {
    code: string;
    detailType: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE';
    specName: string;
    specValue: string;
}

export interface ProductVariantDto {
    code: string;
    productCode: string;
    sku: string;
    attributes: string; // JSON string
    price: number;
    stockQuantity: number;
    isActive: boolean;
}


export interface ProductDto {
    code: string;
    name: string;
    description?: string;
    price: number;
    wholesalePrice?: number;
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
    brand?: string;
    isFeatured?: boolean;
    isNew?: boolean;
    countryOfOrigin?: string;
    baseUnit?: string;
    binLocation?: string;
    createdAt?: string;
    productImages: ProductImageDto[];
    productUnits?: ProductUnitDto[];
    details?: ProductDetailDto[];
    variants?: ProductVariantDto[];
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
    productUnits?: ProductUnitDto[];
    variants?: Array<{
        sku: string;
        attributes: string;
        price: number;
        stockQuantity: number;
    }>;
}

export interface UpdateProductRequest extends Partial<CreateProductRequest> {
    isActive?: boolean;
}

// ============ Category Types ============
export interface CategoryDto {
    code: string;
    name: string;
    description?: string;
    imageURL?: string;
    parentCode?: string | null;
    isActive?: boolean;
}

export interface CreateCategoryRequest {
    name: string;
    description?: string;
    imageURL?: string;
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
