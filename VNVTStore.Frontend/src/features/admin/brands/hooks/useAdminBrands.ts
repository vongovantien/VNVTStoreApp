import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { brandService } from '@/services/brandService';
import { SearchCondition, SearchParams } from '@/services/baseService';

export const ADMIN_BRAND_KEYS = {
    all: ['admin', 'brands'] as const,
    list: (params: Record<string, unknown>) => [...ADMIN_BRAND_KEYS.all, 'list', params] as const,
    detail: (code: string) => [...ADMIN_BRAND_KEYS.all, 'detail', code] as const,
    stats: ['admin', 'brands', 'stats'] as const,
};

export function useAdminBrands(params: {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    isActive?: string;
}) {
    const filters: { field: string; value: string | boolean; operator?: SearchCondition }[] = [];
    if (params.isActive) filters.push({ field: 'IsActive', value: params.isActive === 'true', operator: SearchCondition.Equal });
    if (params.search) filters.push({ field: 'Name', value: params.search, operator: SearchCondition.Contains });

    return useQuery({
        queryKey: ADMIN_BRAND_KEYS.list(params),
        queryFn: async () => {
            const searchParams: SearchParams = {
                filters: filters.length > 0 ? filters : undefined,
            };
            if (params.pageIndex !== undefined) searchParams.pageIndex = params.pageIndex;
            if (params.pageSize !== undefined) searchParams.pageSize = params.pageSize;

            const response = await brandService.search(searchParams);

            if (!response.success) {
                throw new Error(response.message || 'Failed to fetch brands');
            }

            return {
                items: response.data?.items || [],
                totalItems: response.data?.totalItems || 0,
                totalPages: Math.ceil((response.data?.totalItems || 0) / (params.pageSize || 10)),
                pageIndex: params.pageIndex || 1,
                pageSize: params.pageSize || 10
            };
        },
        placeholderData: keepPreviousData,
        staleTime: 1000 * 60,
    });
}
// Note: brandService doesn't have a getStats endpoint yet, we'll use local stats or empty
export function useBrandStats() {
    return { data: null, isLoading: false };
}
