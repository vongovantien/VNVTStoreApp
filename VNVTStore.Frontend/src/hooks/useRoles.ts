import { useQuery } from '@tanstack/react-query';
import { roleService } from '@/services/roleService';
import type { SearchParams } from '@/services/baseService';

export const useRoles = (params: SearchParams) => {
    return useQuery({
        queryKey: ['roles', params],
        queryFn: async () => {
            const response = await roleService.search(params);
            return response;
        },
        placeholderData: (previousData) => previousData,
    });
};

export const useRole = (code: string) => {
    return useQuery({
        queryKey: ['role', code],
        queryFn: () => roleService.getByCode(code),
        enabled: !!code,
    });
};

export const usePermissions = () => {
    return useQuery({
        queryKey: ['permissions-all'],
        queryFn: () => import('@/services/permissionService').then(m => m.permissionService.getAll())
    });
};
