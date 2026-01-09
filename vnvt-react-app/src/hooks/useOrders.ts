/**
 * useOrders hook - React Query hooks for Order operations
 */
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService, type UpdateOrderRequest } from '@/services/orderService';

export function useOrders(params?: {
    pageIndex?: number;
    pageSize?: number;
    status?: string;
}) {
    return useQuery({
        queryKey: ['orders', params],
        queryFn: () => orderService.search({
            pageIndex: params?.pageIndex || 1,
            pageSize: params?.pageSize || 10,
            search: params?.status,
            searchField: params?.status ? 'status' : undefined
        }),
        select: (response) => ({
            orders: response.data?.items || [],
            totalPages: response.data?.totalPages || 1,
            totalItems: response.data?.totalItems || 0,
            pageNumber: response.data?.pageNumber || 1
        })
    });
}

export function useAdminOrders(params?: {
    pageIndex?: number;
    pageSize?: number;
    status?: string;
    search?: string;
}) {
    return useQuery({
        queryKey: ['admin-orders', params],
        queryFn: () => orderService.search({
            pageIndex: params?.pageIndex || 1,
            pageSize: params?.pageSize || 10,
            search: params?.status || params?.search,
            searchField: params?.status ? 'status' : params?.search ? 'orderNumber' : undefined
        }),
        select: (response) => ({
            orders: response.data?.items || [],
            totalPages: response.data?.totalPages || 1,
            totalItems: response.data?.totalItems || 0,
            pageNumber: response.data?.pageNumber || 1
        })
    });
}

export function useUpdateOrderStatus() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ code, status }: { code: string; status: string }) =>
            orderService.update(code, { status }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['orders'] });
            queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
            queryClient.invalidateQueries({ queryKey: ['dashboard-stats'] });
        }
    });
}
