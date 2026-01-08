import { apiClient, type ApiResponse } from './api';

// ============ API DTOs ============
export interface AddressDto {
    id: string; // or code
    userCode?: string;
    fullName: string;
    phone: string;
    category: string; // Home/Work
    street: string;
    ward: string;
    district: string;
    city: string;
    isDefault: boolean;
}

export interface UserProfileDto {
    code: string;
    username: string;
    email: string;
    fullName: string;
    phoneNumber: string;
    avatar: string;
    role: string;
    createdAt: string;
}

export interface UpdateProfileRequest {
    fullName: string;
    phoneNumber: string;
    email: string; // Email update might require verification, but let's include it
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
    confirmNewPassword: string;
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

export interface UpdateAddressRequest extends CreateAddressRequest {
    id: string;
}

// ============ Service ============
export const userService = {
    // Profile
    async getProfile(): Promise<ApiResponse<UserProfileDto>> {
        return apiClient.get<UserProfileDto>('/users/profile');
    },

    async updateProfile(data: UpdateProfileRequest): Promise<ApiResponse<UserProfileDto>> {
        return apiClient.put<UserProfileDto>('/users/profile', data);
    },

    async changePassword(data: ChangePasswordRequest): Promise<ApiResponse<boolean>> {
        return apiClient.put<boolean>('/users/change-password', data);
    },

    // Addresses
    async getMyAddresses(): Promise<ApiResponse<AddressDto[]>> {
        return apiClient.get<AddressDto[]>('/addresses');
    },

    async createAddress(data: CreateAddressRequest): Promise<ApiResponse<string>> {
        return apiClient.post<string>('/addresses', data);
    },

    async updateAddress(id: string, data: UpdateAddressRequest): Promise<ApiResponse<boolean>> {
        return apiClient.put<boolean>(`/addresses/${id}`, data);
    },

    async deleteAddress(id: string): Promise<ApiResponse<boolean>> {
        return apiClient.delete<boolean>(`/addresses/${id}`);
    },

    async setDefaultAddress(id: string): Promise<ApiResponse<boolean>> {
        return apiClient.put<boolean>(`/addresses/${id}/set-default`);
    }
};

export default userService;
