import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import { productService, CreateProductRequest, UpdateProductRequest, ProductDto } from '@/services/productService';
import { SearchParams } from '@/services/baseService';
import { Product } from '@/types';
import { getImageUrl } from '@/utils/format';

export const ADMIN_PRODUCT_KEYS = {
    all: ['admin', 'products'] as const,
    list: (params: SearchParams) => [...ADMIN_PRODUCT_KEYS.all, 'list', params] as const,
    detail: (code: string) => [...ADMIN_PRODUCT_KEYS.all, 'detail', code] as const,
};

// Helper to map DTO to Frontend Model (Simplified for Admin)
function mapAdminProduct(dto: ProductDto): Product {
    const images = dto.productImages || [];
    const primaryImg = images.find(img => img.isPrimary) || images[0];

    return {
        code: dto.code,
        name: dto.name,
        slug: dto.code,
        description: dto.description || '',
        price: dto.price,
        image: getImageUrl(primaryImg?.imageURL),
        images: images.map(img => getImageUrl(img.imageURL)),
        productImages: images,
        category: dto.categoryName || '',
        categoryCode: dto.categoryCode || '',
        stock: dto.stockQuantity || 0,

        // Admin specific fields
        costPrice: dto.costPrice,
        wholesalePrice: dto.wholesalePrice,
        isActive: dto.isActive,
        createdAt: dto.createdAt,
        updatedAt: undefined, // Add if available

        // Required by type but maybe not used in list
        rating: 0,
        reviewCount: 0,
        brand: dto.brand || '',
        isFeatured: dto.isFeatured,
        isNew: dto.isNew,

        // Detailed fields (mapped for form population)
        details: dto.details || [],
        productUnits: dto.productUnits || [],
        variants: dto.variants || [],
        baseUnit: dto.baseUnit,
        minStockLevel: dto.minStockLevel,
        binLocation: dto.binLocation,
        supplierCode: undefined, // Need if DTO has it
        vatRate: undefined, // Need if DTO has it
        countryOfOrigin: dto.countryOfOrigin
    };
}

export function useAdminProducts(params: SearchParams) {
    return useQuery({
        queryKey: ADMIN_PRODUCT_KEYS.list(params),
        queryFn: async () => {
            const response = await productService.search(params);
            if (!response.success) {
                throw new Error(response.message || 'Failed to fetch products');
            }
            return {
                items: (response.data?.items || []).map(mapAdminProduct),
                totalItems: response.data?.totalItems || 0,
                totalPages: Math.ceil((response.data?.totalItems || 0) / (params.pageSize || 10)),
                pageIndex: params.pageIndex || 1,
                pageSize: params.pageSize || 10
            };
        },
        placeholderData: keepPreviousData,
        staleTime: 1000 * 60, // 1 minute
    });
}

export function useProductMutation() {
    const queryClient = useQueryClient();

    const create = useMutation({
        mutationFn: (data: CreateProductRequest) => productService.create(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ADMIN_PRODUCT_KEYS.all });
        }
    });

    const update = useMutation({
        mutationFn: ({ id, data }: { id: string; data: UpdateProductRequest }) =>
            productService.update(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ADMIN_PRODUCT_KEYS.all });
        }
    });

    const remove = useMutation({
        mutationFn: (id: string) => productService.delete(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ADMIN_PRODUCT_KEYS.all });
        }
    });

    const importFile = useMutation({
        mutationFn: (file: File) => productService.import(file),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ADMIN_PRODUCT_KEYS.all });
        }
    });

    return { create, update, remove, importFile };
}
