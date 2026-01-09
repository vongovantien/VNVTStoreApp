/**
* Product Service
* Uses only baseService CRUD methods
*/

import { createCrudService, API_ENDPOINTS } from './baseService';

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
    sku?: string;
    weight?: number;
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
export const productService = createCrudService<ProductDto, CreateProductRequest, UpdateProductRequest>({
    endpoint: API_ENDPOINTS.PRODUCTS.BASE,
    resourceName: 'Product'
});

export const categoryService = createCrudService<CategoryDto, CreateCategoryRequest, UpdateCategoryRequest>({
    endpoint: API_ENDPOINTS.CATEGORIES.BASE,
    resourceName: 'Category'
});

export default productService;
