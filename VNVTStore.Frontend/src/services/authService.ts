/**
 * Auth Service
 * Handles authentication API calls (login, register, etc.)
 */

import { apiClient, type ApiResponse } from './api';

// ============ Types ============
export interface LoginRequest {
    username: string;
    password: string;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    fullName?: string;
}

export interface AuthResponseDto {
    token: string;
    refreshToken?: string;
    expiresAt?: string;
    user: {
        code: string;
        username: string;
        email: string;
        fullName?: string;
        role?: string;
    };
}

export interface UserDto {
    code: string;
    username: string;
    email: string;
    fullName?: string;
    role?: string;
}

// ============ Auth Service ============
export const authService = {
    /**
     * Login with username and password
     */
    async login(data: LoginRequest): Promise<ApiResponse<AuthResponseDto>> {
        return apiClient.post<AuthResponseDto>('/auth/login', data);
    },

    /**
     * Register new user
     */
    async register(data: RegisterRequest): Promise<ApiResponse<UserDto>> {
        return apiClient.post<UserDto>('/auth/register', data);
    },

    /**
     * Get current user profile (requires auth)
     */
    async getProfile(): Promise<ApiResponse<UserDto>> {
        return apiClient.get<UserDto>('/auth/me');
    },

    /**
     * Verify email with token
     */
    async verifyEmail(email: string, token: string): Promise<ApiResponse<boolean>> {
        return apiClient.get<boolean>(`/auth/verify-email?email=${encodeURIComponent(email)}&token=${encodeURIComponent(token)}`);
    },

    /**
     * Request password reset link
     */
    async forgotPassword(email: string): Promise<ApiResponse<boolean>> {
        return apiClient.post<boolean>('/auth/forgot-password', { email });
    },

    /**
     * Reset password with token
     */
    async resetPassword(data: { email: string; token: string; newPassword: string }): Promise<ApiResponse<boolean>> {
        return apiClient.post<boolean>('/auth/reset-password', data);
    },
};

export default authService;
