import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { unitService } from '@/services/unitService';
import { SearchParams } from '@/services/baseService';

export const unitKeys = {
    all: ['units'] as const,
    lists: () => [...unitKeys.all, 'list'] as const,
    list: (params: SearchParams) => [...unitKeys.lists(), params] as const,
};

export const useUnits = (params: SearchParams = { pageIndex: 1, pageSize: 10 }) => {
    return useQuery({
        queryKey: unitKeys.list(params),
        queryFn: () => unitService.search(params),
        placeholderData: keepPreviousData,
    });
};
