import { useQuery } from '@tanstack/react-query';
import { supplierService, SupplierDto } from '@/services';

export const useSuppliers = () => {
    return useQuery({
        queryKey: ['suppliers', 'all'], // 'all' to distinguish from paged/searched lists
        queryFn: async () => {
            const response = await supplierService.getAll();
            const items = response.data?.items || [];
            return Array.from(new Map(items.map(item => [item.code, item])).values());
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
};

export const useSuppliersList = (params: { pageIndex: number; pageSize: number; search?: string; isActive?: string }) => {
    return useQuery({
        queryKey: ['suppliers', params],
        queryFn: async () => {
            const response = await supplierService.search({
                pageIndex: params.pageIndex,
                pageSize: params.pageSize,
                search: params.search,
                filters: params.isActive ? [{ field: 'isActive', value: params.isActive }] : [],
            });
            // Deduplicate
            const uniqueItems = Array.from(new Map((response.data?.items || []).map(item => [item.code, item])).values());

            return {
                suppliers: uniqueItems as SupplierDto[],
                totalItems: response.data?.totalItems || 0,
                totalPages: Math.ceil((response.data?.totalItems || 0) / params.pageSize),
            };
        },
    });
};
