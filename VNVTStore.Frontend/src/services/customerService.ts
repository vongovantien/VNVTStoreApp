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

export default customerService;
