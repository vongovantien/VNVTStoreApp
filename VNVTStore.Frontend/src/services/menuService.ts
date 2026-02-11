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
        const response = await apiClient.get<PagedResult<Menu>>(`${API_ENDPOINTS.MENUS.BASE}?pageSize=100`);
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

export default menuService;
