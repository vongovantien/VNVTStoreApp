
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

    describe('create', () => {
        it('should call apiClient.post with /products and payload', async () => {
            const payload = {
                name: 'New Product',
                price: 100,
                images: ['base64string']
            };
            const mockResponse = { success: true, data: { code: 'NEW001', ...payload } };
            mockPost.mockResolvedValue(mockResponse);

            const result = await productService.create(payload);

            // Expect endpoint to be base products endpoint
            expect(mockPost).toHaveBeenCalled();
            // Since API_ENDPOINTS.PRODUCTS.BASE is likely used, and previous search test expected '/search'
            // Create typically posts to the base URL or similar. 
            // Warning: The search test checked for '/search', implying BASE is '/products'.
            // createEntityService uses endpoint.
            // If search logic was: internal search method -> /products/search?
            // createEntityService likely does post(endpoint, data).
            // So we expect post to be called with endpoint.

            // Checking first assertion of search test: 
            // "search uses http.post internally to /search" -> comment in test file.
            // But productService defines endpoint: API_ENDPOINTS.PRODUCTS.BASE.
            // Let's assume endpoint is passed correctly.

            const url = mockPost.mock.calls[0][0];
            const body = mockPost.mock.calls[0][1];

            expect(url).toContain('/products');
            expect(body).toEqual({ PostObject: payload });
            expect(result).toEqual(mockResponse);
        });
    });
});
