import { createEntityService } from './baseService';
import { API_ENDPOINTS } from '@/constants';
import { Permission } from '@/types';
import apiClient, { ApiResponse, PagedResult } from './api';

export const permissionService = {
    ...createEntityService<Permission, Permission, Permission>({
        endpoint: API_ENDPOINTS.PERMISSIONS.BASE,
        resourceName: 'Permission'
    }),
    getAll: async (): Promise<ApiResponse<Permission[]>> => {
        const response = await apiClient.get<PagedResult<Permission>>(API_ENDPOINTS.PERMISSIONS.ALL);
        // The endpoint returns a PagedResult if using BaseApiController.GetAll custom implementation
        // or just the direct array if we implemented it elsewhere.
        // My implementation in PermissionsController uses GetPagedQuery.
        if (response.success && response.data?.items) {
            return {
                ...response,
                data: response.data.items
            };
        }
        return {
            success: response.success,
            message: response.message,
            data: [],
            statusCode: response.statusCode
        };
    }
};

export default permissionService;
