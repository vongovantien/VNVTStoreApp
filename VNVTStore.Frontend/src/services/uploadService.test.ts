import { describe, it, expect, vi, beforeEach, afterEach, type MockInstance } from 'vitest';
import { uploadService } from './uploadService';
import apiClient from '@/services/api'; // Import real apiClient

describe('uploadService', () => {
    let postSpy: MockInstance;

    beforeEach(() => {
        // Spy on the axios instance post method
        postSpy = vi.spyOn(apiClient.instance, 'post');

        // Mock env var using vi.stubEnv
        vi.stubEnv('VITE_API_URL', 'http://test-api.com/api/v1');
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    it('should upload file and prepend base URL to relative path', async () => {
        // Arrange
        const mockFile = new File(['content'], 'test.png', { type: 'image/png' });
        const mockResponse = {
            data: {
                url: '/uploads/test.png'
            }
        };

        postSpy.mockResolvedValue(mockResponse);

        // Act
        const result = await uploadService.upload(mockFile);

        // Assert
        expect(postSpy).toHaveBeenCalledWith('/upload', expect.any(FormData), expect.objectContaining({
            headers: { 'Content-Type': 'multipart/form-data' }
        }));

        // Should prepend root url (http://test-api.com) to /uploads/test.png
        expect(result).toBe('http://test-api.com/uploads/test.png');
    });

    it('should handle API URL without v1 suffix correctly', async () => {
        // Arrange
        vi.stubEnv('VITE_API_URL', 'http://other-api.com');
        const mockFile = new File(['content'], 'test.png', { type: 'image/png' });
        const mockResponse = { data: { url: '/uploads/image.jpg' } };
        postSpy.mockResolvedValue(mockResponse);

        // Act
        const result = await uploadService.upload(mockFile);

        // Assert
        expect(result).toBe('http://other-api.com/uploads/image.jpg');
    });
});
