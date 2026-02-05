import { describe, it, expect, vi, beforeEach, Mock } from 'vitest';
import { promotionService, type CreatePromotionRequest, type UpdatePromotionRequest } from '../promotionService';
import { apiClient, SearchCondition } from '../api';

// Mock API Client
vi.mock('../api', () => {
    const mockFn = {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        delete: vi.fn(),
    };
    return {
        apiClient: mockFn,
        SearchCondition: {
            Equal: 0,
        },
        default: mockFn,
    };
});

describe('promotionService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const mockGet = apiClient.get as Mock;
    const mockPost = apiClient.post as Mock;

    describe('getByCode', () => {
        it('should call direct API first', async () => {
            const code = 'SAVE10';
            const mockResponse = { success: true, data: { code: 'SAVE10', discountValue: 10 } };
            mockGet.mockResolvedValueOnce(mockResponse);

            const result = await promotionService.getByCode(code);

            expect(mockGet).toHaveBeenCalledWith(`/promotions/code/${code}`);
            expect(result).toEqual(mockResponse);
        });

        it('should fallback to search if direct call fails', async () => {
            const code = 'SAVE10';
            const mockErrorResponse = { success: false, statusCode: 404 };
            const mockSearchResponse = {
                success: true,
                data: {
                    items: [{ code: 'SAVE10', discountValue: 10 }],
                    totalItems: 1
                }
            };

            mockGet.mockResolvedValueOnce(mockErrorResponse);
            mockPost.mockResolvedValueOnce(mockSearchResponse);

            const result = await promotionService.getByCode(code);

            expect(mockGet).toHaveBeenCalledWith(`/promotions/code/${code}`);
            expect(mockPost).toHaveBeenCalledWith('/promotions/search', {
                pageIndex: 1,
                pageSize: 1,
                searching: [{ searchField: 'code', searchValue: code, searchCondition: SearchCondition.Equal }]
            });
            expect(result.success).toBe(true);
            expect(result.data).toEqual(mockSearchResponse.data.items[0]);
        });
    });

    describe('CRUD operations', () => {
        it('should create promotion with correct wrapper', async () => {
            const payload = { code: 'NEW', name: 'New Promo' };
            mockPost.mockResolvedValueOnce({ success: true, data: payload });
            await promotionService.create(payload as unknown as CreatePromotionRequest);
            expect(mockPost).toHaveBeenCalledWith('/promotions', { PostObject: payload });
        });

        it('should update promotion with correct wrapper', async () => {
            const id = '123';
            const payload = { name: 'Updated' };
            (apiClient.put as Mock).mockResolvedValueOnce({ success: true, data: payload });
            await promotionService.update(id, payload as unknown as UpdatePromotionRequest);
            expect(apiClient.put).toHaveBeenCalledWith(`/promotions/${id}`, { PostObject: payload });
        });
    });
});
