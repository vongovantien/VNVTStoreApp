import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { categoryService } from '@/services/productService';
import { SearchCondition, SearchParams } from '@/services/baseService';
import { CATEGORY_LIST_FIELDS } from '@/constants/fieldConstants';

export const ADMIN_CATEGORY_KEYS = {
    all: ['admin', 'categories'] as const,
    list: (params: Record<string, unknown>) => [...ADMIN_CATEGORY_KEYS.all, 'list', params] as const,
    detail: (code: string) => [...ADMIN_CATEGORY_KEYS.all, 'detail', code] as const,
    stats: ['admin', 'categories', 'stats'] as const,
};

export function useAdminCategories(params: {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    parentCode?: string;
    isActive?: string;
    fields?: string[];
}) {
    const { fields = CATEGORY_LIST_FIELDS, ...searchParams } = params;

    const filters: { field: string; value: string | boolean; operator?: SearchCondition }[] = [];
    if (searchParams.parentCode) filters.push({ field: 'parentCode', value: searchParams.parentCode, operator: SearchCondition.Equal });
    if (searchParams.isActive) filters.push({ field: 'isActive', value: searchParams.isActive === 'true', operator: SearchCondition.Equal });
    if (searchParams.search) filters.push({ field: 'name', value: searchParams.search, operator: SearchCondition.Contains });

    return useQuery({
        queryKey: ADMIN_CATEGORY_KEYS.list({ ...searchParams, fields }),
        queryFn: async () => {
            const finalSearchParams: SearchParams = {
                filters: filters.length > 0 ? filters : undefined,
                fields,
            };
            if (searchParams.pageIndex !== undefined) finalSearchParams.pageIndex = searchParams.pageIndex;
            if (searchParams.pageSize !== undefined) finalSearchParams.pageSize = searchParams.pageSize;

            const response = await categoryService.search(finalSearchParams);

            if (!response.success) {
                throw new Error(response.message || 'Failed to fetch categories');
            }

            return {
                items: response.data?.items || [],
                totalItems: response.data?.totalItems || 0,
                totalPages: Math.ceil((response.data?.totalItems || 0) / (searchParams.pageSize || 10)),
                pageIndex: searchParams.pageIndex || 1,
                pageSize: searchParams.pageSize || 10
            };
        },
        placeholderData: keepPreviousData,
        staleTime: 1000 * 60, // 1 minute
    });
}

export function useCategoryStats() {
    return useQuery({
        queryKey: ADMIN_CATEGORY_KEYS.stats,
        queryFn: async () => {
            return await categoryService.getStats();
        },
        staleTime: 1000 * 60,
    });
}
