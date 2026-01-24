/**
 * Customer Service
 */

import { createEntityService, API_ENDPOINTS } from './baseService';

// ============ Types ============
export interface CustomerDto {
    code: string;
    username?: string;
    email: string;
    fullName: string;
    phone?: string; // Renamed from phoneNumber
    address?: string;
    city?: string;
    district?: string;
    ward?: string;
    role: string;
    createdAt: string;
    ordersCount?: number;
    totalSpent?: number;
    isActive: boolean;
    isEmailVerified: boolean;
    lastLogin?: string;
}

export interface CreateCustomerRequest {
    username: string;
    email: string;
    password?: string;
    fullName: string;
    phone?: string; // Renamed from phoneNumber
    role?: string;
    isActive?: boolean;
}

export interface UpdateCustomerRequest {
    fullName?: string;
    phone?: string; // Renamed from phoneNumber
    email?: string;
    role?: string;
    isActive?: boolean;
    password?: string;
    // Address fields removed as they belong to AddressService or need separate handling
}

// ============ Service ============
export const customerService = createEntityService<CustomerDto, CreateCustomerRequest, UpdateCustomerRequest>({
    endpoint: API_ENDPOINTS.USERS.BASE,
    resourceName: 'Customer'
});

// Extend with stats
const extendedService = {
    ...customerService,
    getStats: async () => {
        const [totalRes, activeRes, unverifiedRes] = await Promise.all([
            // Total
            customerService.search({ pageIndex: 1, pageSize: 1 }),
            // Active
            customerService.search({
                pageIndex: 1,
                pageSize: 1,
                filters: [{ field: 'isActive', value: 'true' }]
            }),
            // Unverified (Not Activated)
            customerService.search({
                pageIndex: 1,
                pageSize: 1,
                filters: [{ field: 'isEmailVerified', value: 'false' }]
            })
        ]);

        return {
            total: totalRes.data?.totalItems || 0,
            active: activeRes.data?.totalItems || 0,
            unverified: unverifiedRes.data?.totalItems || 0
        };
    }
};

export const customerServiceApi = extendedService;
export default extendedService;
