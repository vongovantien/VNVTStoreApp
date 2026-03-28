import { useQuery, useInfiniteQuery, keepPreviousData } from '@tanstack/react-query';
import { Product } from '@/types';
import { productService, categoryService, type ProductDto, type CategoryDto } from '@/services/productService';
import { SearchCondition, type SearchParams } from '@/services/baseService';
import { CATEGORY_LIST_FIELDS, PRODUCT_LIST_FIELDS } from '@/constants/fieldConstants';
import { useDiagnosticStore } from '@/store/diagnosticStore';
import { productKeys } from './keys';
import { mapProductDtoToProduct } from './productMapper';

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
        enabled = true,
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
            search: search ?? '',
            filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
            searchField: 'Name',
            fields
        }),
        enabled: !!enabled,
        select: (response) => {
            if (response.success && response.data) {
                return (response.data.items as unknown as CategoryDto[]) || [];
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
            pageIndex: searchParams.pageIndex ?? 1,
            pageSize: searchParams.pageSize ?? 10,
            ...(filters.length > 0 && { filters }),
            fields,
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
    inStockOnly?: boolean; // Feature 4: filter out-of-stock products
    isNewArrivals?: boolean; // Feature 36: Filter by Date Added
}) {
    const { enabled = true, fields = PRODUCT_LIST_FIELDS, ...searchParams } = params;

    // Build filters dynamically
    const filters: { field: string; value: string | string[] | number | number[] | boolean; operator?: SearchCondition }[] = [];

    // 1. Generic/Dynamic params (excluding specific ones)
    Object.entries(searchParams).forEach(([key, value]) => {
        if (['pageIndex', 'pageSize', 'search', 'sortField', 'sortDir', 'brands', 'minPrice', 'maxPrice', 'rating', 'category', 'ids', 'priceType', 'inStockOnly', 'isNewArrivals'].includes(key)) return;
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

    // 5. In-Stock Only (Feature 4)
    if (searchParams.inStockOnly) {
        filters.push({ field: 'StockQuantity', value: 0, operator: SearchCondition.GreaterThan });
    }

    // Feature 36: Filter by Date Added ("New Arrivals" - last 7 days)
    if (searchParams.isNewArrivals) {
        const sevenDaysAgo = new Date();
        sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
        filters.push({
            field: 'CreatedAt',
            value: sevenDaysAgo.toISOString(),
            operator: SearchCondition.GreaterThanEqual
        });
    }

    // Feature 32: Exclude Keyword Search
    // Parse search string to separate "positive" and "-negative" terms
    const rawSearch = searchParams.search || '';
    const terms = rawSearch.split(' ');
    const includeTerms = terms.filter(t => !t.startsWith('-')).join(' ');
    const excludeTerms = terms.filter(t => t.startsWith('-') && t.length > 1).map(t => t.substring(1).toLowerCase());

    // Tracer Injection
    if (enabled && (includeTerms || excludeTerms.length > 0 || searchParams.category || searchParams.brands?.length)) {
        useDiagnosticStore.getState().track({
            module: 'SHOP',
            eventType: 'QUERY_DETECTION',
            description: `Detected complex product query: ${includeTerms} [Excluded: ${excludeTerms.join(', ')}]`,
            payload: { ...searchParams, filtersCount: filters.length },
            severity: 'INFO'
        });
    }

    return useQuery({
        queryKey: productKeys.list({ ...searchParams, fields, includeTerms, excludeTerms }), // distinct key
        queryFn: () => productService.search({
            pageIndex: searchParams.pageIndex ?? 1,
            pageSize: searchParams.pageSize ?? 12,
            search: includeTerms, // Only send positive terms to API
            searchField: 'name',
            ...(searchParams.sortField && { sortBy: searchParams.sortField }),
            sortDesc: searchParams.sortDir === 'desc',
            ...(filters.length > 0 && { filters }),
            fields,
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

                // Deduplicate items
                let uniqueItems = Array.from(new Map((response.data.items || []).map(item => [item.code, item])).values());

                // Feature 32: Client-side exclusion filter
                if (excludeTerms.length > 0) {
                    uniqueItems = uniqueItems.filter(item => {
                        const searchContent = `${item.name} ${item.description || ''} ${item.categoryName || ''}`.toLowerCase();
                        // If item contains ANY excluded term, filter it out
                        return !excludeTerms.some(term => searchContent.includes(term));
                    });
                    // Note: This reduces the page size client-side, which is a known limitation of this "simple" implementation
                }

                const totalPages = Math.ceil(totalItems / pageSize);

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
 * Hook for infinite scrolling products
 */
export function useInfiniteProducts(params: {
    pageSize?: number | undefined;
    search?: string | undefined;
    sortField?: string | undefined;
    sortDir?: 'asc' | 'desc' | undefined;
    category?: string | undefined;
    brands?: string[] | undefined;
    minPrice?: number | undefined;
    maxPrice?: number | undefined;
    rating?: number | undefined;
    discount?: number | undefined;
    priceType?: 'all' | 'fixed' | 'contact' | undefined;
    enabled?: boolean | undefined;
    fields?: string[] | undefined;
    inStockOnly?: boolean | undefined;
    isNewArrivals?: boolean | undefined;
}) {
    const { enabled = true, fields = PRODUCT_LIST_FIELDS, pageSize = 12, ...searchParams } = params;

    return useInfiniteQuery({
        queryKey: [...productKeys.lists(), 'infinite', { ...searchParams, fields, pageSize }],
        queryFn: async ({ pageParam = 1 }) => {
            console.log(`[useInfiniteProducts] Querying page ${pageParam}`, searchParams);
            // Build filters (logic similar to useProducts but reusable)
            const filters: NonNullable<SearchParams['filters']> = [];
            if (searchParams.category) {
                if (searchParams.category.includes(',')) {
                    filters.push({ field: 'CategoryCode', value: searchParams.category.split(','), operator: SearchCondition.In });
                } else {
                    filters.push({ field: 'CategoryCode', value: searchParams.category, operator: SearchCondition.Equal });
                }
            }
            if (searchParams.brands?.length) filters.push({ field: 'Brand', value: searchParams.brands, operator: SearchCondition.In });
            if (searchParams.minPrice) filters.push({ field: 'Price', value: searchParams.minPrice, operator: SearchCondition.GreaterThanEqual });
            if (searchParams.maxPrice) filters.push({ field: 'Price', value: searchParams.maxPrice, operator: SearchCondition.LessThanEqual });
            if (searchParams.rating) filters.push({ field: 'Rating', value: searchParams.rating, operator: SearchCondition.GreaterThanEqual });
            if (searchParams.discount) filters.push({ field: 'Discount', value: searchParams.discount, operator: SearchCondition.GreaterThanEqual });
            if (searchParams.priceType && searchParams.priceType !== 'all') {
                filters.push({ field: 'PriceType', value: searchParams.priceType, operator: SearchCondition.Equal });
            }
            if (searchParams.inStockOnly) {
                filters.push({ field: 'StockQuantity', value: 0, operator: SearchCondition.GreaterThan });
            }
            if (searchParams.isNewArrivals) {
                 filters.push({ field: 'IsNew', value: true, operator: SearchCondition.Equal });
            }

            // Feature 32: Exclude Logic (Simple Split)
            const rawSearch = searchParams.search || '';
            const terms = rawSearch.split(' ');
            const includeTerms = terms.filter(t => !t.startsWith('-')).join(' ');
            const excludeTerms = terms.filter(t => t.startsWith('-') && t.length > 1).map(t => t.substring(1).toLowerCase());

            // Tracer Injection
            useDiagnosticStore.getState().track({
                module: 'SHOP',
                eventType: 'INFINITE_LOAD',
                description: `Infinite query page ${pageParam} triggered for ${includeTerms || 'all products'}`,
                payload: { pageParam, searchParams },
                severity: 'INFO'
            });

            const response = await productService.search({
                pageIndex: pageParam,
                pageSize,
                search: includeTerms,
                ...(searchParams.sortField && { sortBy: searchParams.sortField }),
                sortDesc: searchParams.sortDir === 'desc',
                ...(filters.length > 0 && { filters }),
                fields,
            });

            if (response.success && response.data) {
                let items = response.data.items || [];

                // Feature 32: Client-side exclude filter
                if (excludeTerms.length > 0) {
                    items = items.filter(item => {
                        const searchContent = `${item.name} ${item.description || ''} ${item.categoryName || ''}`.toLowerCase();
                        return !excludeTerms.some(term => searchContent.includes(term));
                    });
                }

                return {
                    items: items.map(item => mapProductDtoToProduct(item as ProductDto)),
                    nextPage: pageParam < Math.ceil(response.data.totalItems / pageSize) ? pageParam + 1 : undefined,
                    totalItems: response.data.totalItems,
                };
            }
            return { items: [], nextPage: undefined, totalItems: 0 };
        },
        initialPageParam: 1,
        getNextPageParam: (lastPage) => lastPage.nextPage,
        enabled,
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
