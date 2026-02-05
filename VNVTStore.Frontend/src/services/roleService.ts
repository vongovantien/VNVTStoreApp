import { createEntityService } from './baseService';
import { API_ENDPOINTS } from '@/constants';
import { Role } from '@/types';

export interface CreateRoleRequest {
    name: string;
    description?: string;
    isActive: boolean;
    permissionCodes: string[];
}

export type UpdateRoleRequest = Partial<CreateRoleRequest>;

export const roleService = {
    ...createEntityService<Role, CreateRoleRequest, UpdateRoleRequest>({
        endpoint: API_ENDPOINTS.ROLES.BASE,
        resourceName: 'Role'
    })
};

export default roleService;
