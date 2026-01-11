/**
* Product Service
* Uses only baseService CRUD methods
*/

import { createEntityService, API_ENDPOINTS } from './baseService';

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
    sku?: string;
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
    stock?: number; // Added to support legacy or partial mapping if needed
    sku?: string;
    weight?: number;
    color?: string;
    power?: string;
    voltage?: string;
    material?: string;
    size?: string;
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
    parentCode?: string;
    isActive?: boolean;
}

export interface CreateCategoryRequest {
    name: string;
    description?: string;
    parentCode?: string;
}

export interface UpdateCategoryRequest extends Partial<CreateCategoryRequest> {
    isActive?: boolean;
}

// ============ Services ============
export const productService = createEntityService<ProductDto, CreateProductRequest, UpdateProductRequest>({
    endpoint: API_ENDPOINTS.PRODUCTS.BASE,
    resourceName: 'Product'
});

export const categoryService = createEntityService<CategoryDto, CreateCategoryRequest, UpdateCategoryRequest>({
    endpoint: API_ENDPOINTS.CATEGORIES.BASE,
    resourceName: 'Category'
});

export default productService;
