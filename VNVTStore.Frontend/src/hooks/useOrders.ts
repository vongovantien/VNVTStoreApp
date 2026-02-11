/**
 * useOrders hook - React Query hooks for Order operations
 */
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService } from '@/services/orderService';
import { SearchCondition } from '@/services/baseService';

import { ORDER_LIST_FIELDS } from '@/constants/fieldConstants';

export function useOrders(params?: {
    pageIndex?: number;
    pageSize?: number;
    status?: string;
    fields?: string[];
}) {
    const { fields = ORDER_LIST_FIELDS } = params || {};
    return useQuery({
        queryKey: ['orders', params],
        queryFn: () => orderService.search({
            pageIndex: params?.pageIndex || 1,
            pageSize: params?.pageSize || 10,
            search: params?.status,
            searchField: params?.status ? 'status' : undefined,
            fields: fields || ORDER_LIST_FIELDS
        }),
        select: (response) => {
            const pageSize = params?.pageSize || 10;
            const pageIndex = params?.pageIndex || 1;
            const totalItems = response.data?.totalItems || 0;
            const totalPages = Math.ceil(totalItems / pageSize);

            // Deduplicate
            const uniqueItems = Array.from(new Map((response.data?.items || []).map(item => [item.code, item])).values());

            return {
                orders: uniqueItems,
                totalPages,
                totalItems,
                pageNumber: pageIndex
            };
        }
    });
}

export function useAdminOrders(params?: {
    pageIndex?: number;
    pageSize?: number;
    filters?: Record<string, unknown>;
    fields?: string[];
}) {
    const { fields = ORDER_LIST_FIELDS } = params || {};
    const searchFilters: { field: string; value: string; operator?: SearchCondition }[] = [];
    const searchTerm = params?.filters?.search as string;

    // Convert filters to array for backend
    if (params?.filters) {
        Object.entries(params.filters).forEach(([key, value]) => {
            if (value && key !== 'search') {
                searchFilters.push({ field: key, value: String(value) });
            }
        });
    }

    return useQuery({
        queryKey: ['admin-orders', params],
        queryFn: () => orderService.search({
            pageIndex: params?.pageIndex || 1,
            pageSize: params?.pageSize || 10,
            search: searchTerm,
            searchField: 'all', // Backend will search across default fields if not specified
            filters: searchFilters.length > 0 ? searchFilters : undefined,
            fields: fields || ORDER_LIST_FIELDS
        }),
        select: (response) => {
            const pageSize = params?.pageSize || 10;
            const pageIndex = params?.pageIndex || 1;
            const totalItems = response.data?.totalItems || 0;
            const totalPages = Math.ceil(totalItems / pageSize);

            // Deduplicate
            const uniqueItems = Array.from(new Map((response.data?.items || []).map(item => [item.code, item])).values());

            return {
                orders: uniqueItems,
                totalPages,
                totalItems,
                pageNumber: pageIndex
            };
        }
    });
}

export function useUpdateOrderStatus() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ code, status }: { code: string; status: string }) =>
            orderService.updateStatus(code, status),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['orders'] });
            queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
            queryClient.invalidateQueries({ queryKey: ['dashboard-stats'] });
        }
    });
}
export function useOrder(code: string) {
    return useQuery({
        queryKey: ['order', code],
        queryFn: async () => {
            const res = await orderService.getByCode(code);
            if (!res.success || !res.data) {
                throw new Error(res.message || 'Failed to fetch order');
            }
            return res.data;
        },
        enabled: !!code
    });
}
