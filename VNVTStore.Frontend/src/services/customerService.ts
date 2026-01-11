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
    phoneNumber?: string;
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
    phoneNumber?: string;
    role?: string;
}

export interface UpdateCustomerRequest {
    fullName?: string;
    phoneNumber?: string;
    address?: string;
    city?: string;
    district?: string;
    ward?: string;
    isActive?: boolean;
}

// ============ Service ============
export const customerService = createEntityService<CustomerDto, CreateCustomerRequest, UpdateCustomerRequest>({
    endpoint: API_ENDPOINTS.USERS.BASE,
    resourceName: 'Customer'
});

export default customerService;
