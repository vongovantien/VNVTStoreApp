
import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { productService } from '../productService';
import { apiClient } from '../api';

// Mock API Client
vi.mock('../api', () => {
    const mockFn = {
        get: vi.fn(),
        post: vi.fn(),
    };
    return {
        apiClient: mockFn,
        default: mockFn,
    };
});

describe('productService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const mockGet = apiClient.get as Mock;
    const mockPost = apiClient.post as Mock;

    describe('search', () => {
        it('should call apiClient.post with /products/search', async () => {
            const params = { pageIndex: 1, pageSize: 10 };
            const mockResponse = { success: true, data: [] };

            // search uses http.post internally to /search
            // endpoint is /products
            // so url should be /products/search
            const mockPost = apiClient.post as any;
            mockPost.mockResolvedValue(mockResponse);

            const result = await productService.search(params);

            expect(mockPost).toHaveBeenCalled();
            expect(mockPost.mock.calls[0][0]).toContain('/search');
            expect(result).toEqual(mockResponse);
        });
    });

    describe('getByCode', () => {
        it('should call apiClient.get with /products/:code', async () => {
            const code = '123';
            const mockResponse = { success: true, data: { code: '123' } };
            mockGet.mockResolvedValue(mockResponse);

            const result = await productService.getByCode(code);

            // endpoint + /code
            // check first arg contains code
            expect(mockGet).toHaveBeenCalled();
            expect(mockGet.mock.calls[0][0]).toContain(code);
            expect(result).toEqual(mockResponse);
        });
    });
});
