/**
 * User Service
 * Uses only baseService CRUD methods
 */

import { createCrudService, API_ENDPOINTS } from './baseService';

// ============ User Types ============
export interface UserProfileDto {
    code: string;
    username: string;
    email: string;
    fullName: string;
    phoneNumber: string;
    avatar?: string;
    role: string;
    createdAt: string;
}

export interface UpdateProfileRequest {
    fullName?: string;
    phoneNumber?: string;
    email?: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
    confirmNewPassword: string;
}

// ============ Address Types ============
export interface AddressDto {
    code: string;
    userCode?: string;
    fullName: string;
    phone: string;
    category: string;
    street: string;
    ward: string;
    district: string;
    city: string;
    isDefault: boolean;
}

export interface CreateAddressRequest {
    fullName: string;
    phone: string;
    category: string;
    street: string;
    ward: string;
    district: string;
    city: string;
    isDefault: boolean;
}

export interface UpdateAddressRequest extends Partial<CreateAddressRequest> { }

// ============ Services ============
export const userService = createCrudService<UserProfileDto, UpdateProfileRequest, UpdateProfileRequest>({
    endpoint: API_ENDPOINTS.USERS.BASE,
    resourceName: 'User'
});

export const addressService = createCrudService<AddressDto, CreateAddressRequest, UpdateAddressRequest>({
    endpoint: API_ENDPOINTS.ADDRESSES.BASE,
    resourceName: 'Address'
});

export default userService;
