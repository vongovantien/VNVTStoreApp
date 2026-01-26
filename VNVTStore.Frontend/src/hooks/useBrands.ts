import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { brandService } from '@/services/brandService';
import { SearchParams } from '@/services/baseService';

export const brandKeys = {
    all: ['brands'] as const,
    lists: () => [...brandKeys.all, 'list'] as const,
    list: (params: SearchParams) => [...brandKeys.lists(), params] as const,
};

export const useBrands = (params: SearchParams = { pageIndex: 1, pageSize: 10 }) => {
    return useQuery({
        queryKey: brandKeys.list(params),
        queryFn: () => brandService.search(params),
        placeholderData: keepPreviousData,
    });
};
