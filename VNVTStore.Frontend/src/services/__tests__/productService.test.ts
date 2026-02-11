import { describe, it, expect, vi, beforeEach } from 'vitest';
import { productService } from '../productService';
import apiClient from '../api';
import { API_ENDPOINTS } from '../baseService';

// Mock apiClient
vi.mock('../api', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        delete: vi.fn(),
    }
}));

describe('productService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe('import', () => {
        it('should call api.post with correct parameters', async () => {
            const file = new File(['dummy'], 'test.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            (apiClient.post as unknown as ReturnType<typeof vi.fn>).mockResolvedValue({ success: true, data: 10 });

            const result = await productService.import(file);

            expect(apiClient.post).toHaveBeenCalledTimes(1);
            expect(apiClient.post).toHaveBeenCalledWith(
                `${API_ENDPOINTS.PRODUCTS.BASE}/import`,
                expect.any(FormData),
                expect.objectContaining({
                    headers: {
                        'Content-Type': 'multipart/form-data',
                    }
                })
            );
            expect(result.data).toBe(10);
        });
    });

    describe('getTemplate', () => {
        it('should return the correct template URL', () => {
            const url = productService.getTemplate();
            expect(url).toBe(`${API_ENDPOINTS.PRODUCTS.BASE}/template`);
        });
    });

    describe('getStats', () => {
        it('should return stats when api call is successful', async () => {
            const mockStats = { total: 100, outOfStock: 5, lowStock: 10 };
            (apiClient.get as unknown as ReturnType<typeof vi.fn>).mockResolvedValue({ success: true, data: mockStats });

            const result = await productService.getStats();

            expect(apiClient.get).toHaveBeenCalledWith(`${API_ENDPOINTS.PRODUCTS.BASE}/stats`);
            expect(result).toEqual(mockStats);
        });

        it('should return default stats when api returns 404/error', async () => {
            (apiClient.get as unknown as ReturnType<typeof vi.fn>).mockResolvedValue({ success: false, message: '404 Not Found' });

            const result = await productService.getStats();

            expect(result).toEqual({ total: 0, outOfStock: 0, lowStock: 0 });
        });
    });
});
