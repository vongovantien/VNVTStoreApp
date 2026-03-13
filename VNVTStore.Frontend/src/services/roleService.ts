import { createEntityService, type ApiResponse } from './baseService';
import { API_ENDPOINTS } from '@/constants';
import { Role } from '@/types';

export interface CreateRoleRequest {
    name: string;
    description?: string;
    isActive: boolean;
    permissionCodes: string[];
    menuCodes: string[];
}

export type UpdateRoleRequest = Partial<CreateRoleRequest>;

const baseService = createEntityService<Role, CreateRoleRequest, UpdateRoleRequest>({
    endpoint: API_ENDPOINTS.ROLES.BASE,
    resourceName: 'Role'
});

export const roleService = {
    ...baseService,
    /**
     * Get by code with includeChildren by default for admin detail
     */
    async getByCode(code: string, params?: Record<string, unknown>): Promise<ApiResponse<Role>> {
        return baseService.getByCode(code, {
            includeChildren: true,
            ...params
        });
    }
};

export default roleService;
