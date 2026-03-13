import { createEntityService } from './baseService';
import { API_ENDPOINTS } from '@/constants';
import apiClient, { ApiResponse, PagedResult } from './api';

import { Menu } from '@/types';

export const menuService = {
    ...createEntityService<Menu, Menu, Menu>({
        endpoint: API_ENDPOINTS.MENUS.BASE,
        resourceName: 'Menu'
    }),
    getAll: async (): Promise<ApiResponse<Menu[]>> => {
        const response = await apiClient.post<PagedResult<Menu>>(API_ENDPOINTS.MENUS.SEARCH, {
            pageIndex: 1,
            pageSize: 100
        });

        if (response.success && response.data?.items) {
            return {
                ...response,
                data: response.data.items
            };
        }

        return {
            success: false,
            message: response.message || 'Failed to load menus',
            data: [],
            statusCode: response.statusCode || 500
        };
    }
};

export default menuService;
