import { useQuery } from '@tanstack/react-query';
import { supplierService, SupplierDto } from '@/services';

import { SearchCondition } from '@/services/baseService';

export const useSuppliers = (params?: {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    enabled?: boolean;
    fields?: string[];
}) => {
    const {
        enabled,
        pageIndex = 1,
        pageSize = 1000,
        search,
        fields = ['Code', 'Name', 'IsActive', "CreatedAt"]
    } = params || {};

    return useQuery({
        queryKey: ['suppliers', 'all', { pageIndex, pageSize, search, fields }], // 'all' to distinguish from paged/searched lists
        queryFn: async () => {
            const response = await supplierService.search({
                pageIndex,
                pageSize,
                search,
                filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
                fields
            });
            const items = response.data?.items || [];
            return Array.from(new Map(items.map(item => [item.code, item])).values());
        },
        enabled: enabled,
        staleTime: 5 * 60 * 1000, // 5 minutes
    });
};

export const useSuppliersPaged = (params: any) => {
    return useQuery({
        queryKey: ['suppliers', 'paged', params],
        queryFn: async () => {
            const response = await supplierService.search(params);
            return response.data;
        },
    });
};
