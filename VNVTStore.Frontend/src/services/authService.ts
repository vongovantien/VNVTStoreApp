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
    token?: string;
    refreshToken?: string;
    expiresAt?: string;
    requiresTwoFactor?: boolean;
    twoFactorToken?: string;
    user: {
        code: string;
        username: string;
        email: string;
        fullName?: string;
        role?: string;
        avatar?: string;
        twoFactorEnabled?: boolean;
        permissions: string[];
        menus: string[];
    };
}

export interface UserDto {
    code: string;
    username: string;
    email: string;
    fullName?: string;
    role?: string;
    avatar?: string;
    twoFactorEnabled?: boolean;
    permissions: string[];
    menus: string[];
}

export interface TwoFactorSetupResponse {
    secretKey: string;
    qrCodeUri: string;
    manualEntryKey: string;
}

export interface TwoFactorEnableResponse {
    recoveryCodes: string[];
}

export interface VerifyTwoFactorLoginRequest {
    twoFactorToken: string;
    code: string;
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

    /**
     * Login with Google/Facebook
     */
    async externalLogin(provider: string, token: string): Promise<ApiResponse<AuthResponseDto>> {
        return apiClient.post<AuthResponseDto>('/auth/external-login', { provider, token });
    },

    /**
     * Login as another user (Admin only)
     */
    async impersonate(userCode: string): Promise<ApiResponse<AuthResponseDto>> {
        return apiClient.post<AuthResponseDto>(`/auth/impersonate/${userCode}`);
    },

    /**
     * Setup 2FA (Get QR Code and secret)
     */
    async setupTwoFactor(): Promise<ApiResponse<TwoFactorSetupResponse>> {
        return apiClient.post<TwoFactorSetupResponse>('/auth/2fa/setup');
    },

    /**
     * Enable 2FA with verified TOTP code
     */
    async enableTwoFactor(data: { secretKey: string; code: string }): Promise<ApiResponse<TwoFactorEnableResponse>> {
        return apiClient.post<TwoFactorEnableResponse>('/auth/2fa/enable', data);
    },

    /**
     * Disable 2FA with password & TOTP / recovery code
     */
    async disableTwoFactor(data: { password: string; code: string }): Promise<ApiResponse<boolean>> {
        return apiClient.post<boolean>('/auth/2fa/disable', data);
    },

    /**
     * Verify 2FA at login
     */
    async verifyTwoFactorLogin(data: VerifyTwoFactorLoginRequest): Promise<ApiResponse<AuthResponseDto>> {
        return apiClient.post<AuthResponseDto>('/auth/2fa/verify', data);
    },
};

export default authService;
