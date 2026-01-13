
import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { authService } from '../authService';
import { apiClient } from '../api';

// Mock API Client
vi.mock('../api', () => {
    const mockFn = {
        post: vi.fn(),
    };
    return {
        apiClient: mockFn,
        default: mockFn,
    };
});

describe('authService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const mockPost = apiClient.post as Mock;

    describe('login', () => {
        it('should call apiClient.post with /auth/login', async () => {
            const credentials = { username: 'testuser', password: 'password' };
            const mockResponse = { success: true, data: { token: 'token' } };
            mockPost.mockResolvedValue(mockResponse);

            const result = await authService.login(credentials);

            expect(mockPost).toHaveBeenCalledWith('/auth/login', credentials);
            expect(result).toEqual(mockResponse);
        });
    });

    describe('register', () => {
        it('should call apiClient.post with /auth/register', async () => {
            const data = {
                username: 'testu',
                firstName: 'First',
                lastName: 'Last',
                email: 'test@test.com',
                password: 'password',
                confirmPassword: 'password'
            };
            const mockResponse = { success: true };
            mockPost.mockResolvedValue(mockResponse);

            // Cast to any to bypass strict type check on extra fields like confirmPassword/firstName if they are not in interface but passed to function
            // Or better, match interface. Interface has username, email, password, fullName.
            const serviceData = {
                username: 'testu',
                email: 'test@test.com',
                password: 'password',
                fullName: 'First Last'
            };

            const result = await authService.register(serviceData);

            expect(mockPost).toHaveBeenCalledWith('/auth/register', serviceData);
            expect(result).toEqual(mockResponse);
        });
    });
});
