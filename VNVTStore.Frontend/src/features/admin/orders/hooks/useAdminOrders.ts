import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { orderService, OrderDto } from '@/services/orderService';
import { SearchParams } from '@/services/baseService';

export const ADMIN_ORDER_KEYS = {
    all: ['admin', 'orders'] as const,
    list: (params: SearchParams) => [...ADMIN_ORDER_KEYS.all, 'list', params] as const,
    detail: (code: string) => [...ADMIN_ORDER_KEYS.all, 'detail', code] as const,
    stats: ['admin', 'orders', 'stats'] as const,
};

export function useAdminOrders(params: SearchParams) {
    return useQuery({
        queryKey: ADMIN_ORDER_KEYS.list(params),
        queryFn: async () => {
            const response = await orderService.search(params);
            if (!response.success) {
                throw new Error(response.message || 'Failed to fetch orders');
            }
            return {
                orders: (response.data?.items || []) as OrderDto[],
                totalItems: response.data?.totalItems || 0,
                totalPages: Math.ceil((response.data?.totalItems || 0) / (params.pageSize || 10)),
                pageIndex: params.pageIndex || 1,
                pageSize: params.pageSize || 10
            };
        },
        placeholderData: keepPreviousData,
        staleTime: 1000 * 30, // 30 seconds
    });
}

export function useOrderStats() {
    return useQuery({
        queryKey: ADMIN_ORDER_KEYS.stats,
        queryFn: async () => {
            return await orderService.getStats();
        },
        staleTime: 1000 * 60, // 1 minute
    });
}
