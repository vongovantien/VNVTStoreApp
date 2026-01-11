/**
 * User Service
 * Uses only baseService CRUD methods
 */

import { createEntityService, API_ENDPOINTS } from './baseService';

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
import { apiClient } from './api';
import type { PagedResult } from './baseService';

const baseService = createEntityService<UserProfileDto, UpdateProfileRequest, UpdateProfileRequest>({
    endpoint: API_ENDPOINTS.USERS.BASE,
});

export const userService = {
    ...baseService,
    getProfile: () => apiClient.get<UserProfileDto>(`${API_ENDPOINTS.USERS.BASE}/profile`),
    updateProfile: (data: UpdateProfileRequest) => apiClient.put<UserProfileDto>(`${API_ENDPOINTS.USERS.BASE}/profile`, data),
    getMyAddresses: async () => {
        const res = await apiClient.post<PagedResult<AddressDto>>(`${API_ENDPOINTS.ADDRESSES.BASE}/search`, { PageIndex: 1, PageSize: 100 });
        return { ...res, data: res.data?.items || [] };
    }
};

export const addressService = createEntityService<AddressDto, CreateAddressRequest, UpdateAddressRequest>({
    endpoint: API_ENDPOINTS.ADDRESSES.BASE,
});

export default userService;
