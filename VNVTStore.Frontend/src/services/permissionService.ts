import { createEntityService } from './baseService';
import { API_ENDPOINTS } from '@/constants';
import { Permission } from '@/types';
import { ApiResponse } from './api';

export const permissionService = {
    ...createEntityService<Permission, Permission, Permission>({
        endpoint: API_ENDPOINTS.PERMISSIONS.BASE,
        resourceName: 'Permission'
    }),
    getAll: async (): Promise<ApiResponse<Permission[]>> => {
        const response = await permissionService.search({
            pageIndex: 1,
            pageSize: 1000
        });

        if (response.success && response.data?.items) {
            return {
                ...response,
                data: response.data.items
            };
        }

        return {
            success: false,
            message: response.message || 'Failed to load permissions',
            data: [],
            statusCode: response.statusCode || 500
        };
    }
};

export default permissionService;
