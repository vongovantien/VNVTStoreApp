import { describe, it, expect, vi, beforeEach } from 'vitest';
import { systemConfigService } from './systemConfigService';
import apiClient from './api';

// Mock apiClient
vi.mock('./api', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
    },
}));

describe('systemConfigService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('get calls correct endpoint with key', async () => {
        const mockKey = 'TEST_KEY';
        const mockResponse = {
            success: true,
            message: 'Success',
            data: {
                configKey: mockKey,
                configValue: 'Value',
                isActive: true
            },
            statusCode: 200
        };

        // Mock implementation
        vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

        const result = await systemConfigService.get(mockKey);

        expect(apiClient.get).toHaveBeenCalledWith(`/systemconfig/${mockKey}`);
        expect(result).toEqual(mockResponse);
    });

    it('update calls correct endpoint with payload', async () => {
        const payload = {
            configKey: 'TEST_KEY',
            configValue: 'New Value',
            isActive: true
        };
        const mockResponse = {
            success: true,
            message: 'Updated',
            data: { ...payload, isActive: true },
            statusCode: 200
        };

        vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

        const result = await systemConfigService.update(payload);

        expect(apiClient.post).toHaveBeenCalledWith('/systemconfig', payload);
        expect(result).toEqual(mockResponse);
    });

    it('handles api errors gracefully', async () => {
        const mockError = {
            success: false,
            message: 'Error',
            data: null,
            statusCode: 500
        };
        vi.mocked(apiClient.get).mockResolvedValue(mockError);

        const result = await systemConfigService.get('ERROR_KEY');

        expect(result.success).toBe(false);
        expect(result.message).toBe('Error');
    });
});
