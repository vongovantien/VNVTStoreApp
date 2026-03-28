import { Product, ProductDetail, ProductUnit, ProductVariant } from '@/types';
import { ProductDto, ProductImageDto } from '@/services/productService';
import { getImageUrl } from '@/utils/format';

const tryParse = (json: string) => {
    try { return JSON.parse(json); } catch { return {}; }
};

/**
 * Extract product images from DTO (handles multiple casing variations)
 */
export function getProductImages(item: ProductDto | Record<string, unknown>): ProductImageDto[] | undefined {
    const dto = item as ProductDto;
    const record = item as Record<string, unknown>;
    return (dto.productImages || record.ProductImages || record.product_images) as ProductImageDto[] | undefined;
}

/**
 * Maps ProductDto to Frontend Product model
 * Centralized mapping to avoid duplication (DRY principle)
 */
export function mapProductDtoToProduct(item: ProductDto, options?: { includeDetails?: boolean }): Product {
    const images = getProductImages(item);
    const primaryImg = images?.find((img: ProductImageDto) => img.isPrimary) || images?.[0];

    const record = item as unknown as Record<string, unknown>;
    const imgRecord = primaryImg as unknown as Record<string, unknown> | undefined;
    
    // Build base product with required/primitive fields
    const product: Product = {
        code: item.code || (record.Code as string) || (record.code as string),
        name: item.name || (record.Name as string) || (record.name as string) || 'Sản phẩm không tên',
        slug: item.code || (record.Code as string) || (record.code as string),
        description: item.description || (record.Description as string) || (record.description as string) || '',
        price: item.price !== undefined ? item.price : ((record.Price as number) || 0),
        image: getImageUrl(primaryImg?.imageURL || (imgRecord?.ImageUrl as string) || (imgRecord?.image_url as string)),
        images: images?.map((img) => getImageUrl(img.imageURL || (img as unknown as Record<string, unknown>)?.ImageUrl as string)) || [],
        productImages: images || [],
        category: item.categoryName || (record.CategoryName as string) || (record.category_name as string) || '',
        categoryCode: item.categoryCode || (record.CategoryCode as string) || (record.category_code as string) || '',
        stock: item.stockQuantity !== undefined ? item.stockQuantity : ((record.StockQuantity as number) || (record.stock as number) || 0),
        brand: item.brand || (record.Brand as string) || (record.brand as string) || 'VNVT',
        rating: 5,
        reviewCount: 0,
        createdAt: item.createdAt || (record.CreatedAt as string) || (record.created_at as string) || new Date().toISOString()
    };

    // Handle optional booleans/strings with exactOptionalPropertyTypes compliance
    if (item.isActive !== undefined) product.isActive = item.isActive;
    else product.isActive = true; // Default to active

    if (item.isFeatured !== undefined) product.isFeatured = item.isFeatured;
    if (item.isNew !== undefined) product.isNew = item.isNew;
    if (item.updatedAt !== undefined) product.updatedAt = item.updatedAt;

    // Attributes - Fallback to first variant attributes if main product fields are empty
    const firstVariantAttrs = item.variants?.[0]?.attributes ? tryParse(item.variants[0].attributes) : null;

    product.color = item.color || firstVariantAttrs?.color || firstVariantAttrs?.Color;
    product.power = item.power || firstVariantAttrs?.power || firstVariantAttrs?.Power;
    product.voltage = item.voltage || firstVariantAttrs?.voltage || firstVariantAttrs?.Voltage;
    product.material = item.material || firstVariantAttrs?.material || firstVariantAttrs?.Material;
    product.size = item.size || firstVariantAttrs?.size || firstVariantAttrs?.Size;

    // Add detail-specific fields when fetching single product
    if (options?.includeDetails) {
        if (item.wholesalePrice !== undefined) product.wholesalePrice = item.wholesalePrice;
        if (item.costPrice !== undefined) product.costPrice = item.costPrice;
        if (item.stockQuantity !== undefined) product.stockQuantity = item.stockQuantity;
        if (item.countryOfOrigin !== undefined) product.countryOfOrigin = item.countryOfOrigin;
        if (item.baseUnit !== undefined) product.baseUnit = item.baseUnit;
        if (item.binLocation !== undefined) product.binLocation = item.binLocation;

        product.details = (item.details as unknown as ProductDetail[]) || [];
        product.productUnits = (item.productUnits as unknown as ProductUnit[]) || [];
        product.variants = (item.variants as unknown as ProductVariant[]) || [];
        product.originalPrice = item.price * 1.2;
    }

    return product;
}
