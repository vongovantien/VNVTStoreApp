import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { orderService, type CreateOrderRequest } from '../orderService';
import { apiClient } from '../api';
import { API_ENDPOINTS } from '@/constants';

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
        default: mockFn,
    };
});

describe('orderService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const mockPost = apiClient.post as Mock;

    describe('create', () => {
        it('should call apiClient.post with RAW data (no PostObject wrapper)', async () => {
            const orderData: CreateOrderRequest = {
                fullName: 'Test User',
                phone: '0123456789',
                address: '123 Test St',
                city: 'Test City',
                district: 'Test District',
                ward: 'Test Ward',
                paymentMethod: 'COD',
                couponCode: 'TEST20'
            };

            const mockResponse = { success: true, data: { code: 'ORD-123' } };
            mockPost.mockResolvedValue(mockResponse);

            const result = await orderService.create(orderData);

            // Verify URL
            expect(mockPost).toHaveBeenCalledWith(API_ENDPOINTS.ORDERS.BASE, expect.anything());

            // Verify Payload is NOT wrapped in PostObject
            // It should match orderData exactly
            expect(mockPost).toHaveBeenCalledWith(
                API_ENDPOINTS.ORDERS.BASE,
                expect.objectContaining({
                    fullName: 'Test User',
                    paymentMethod: 'COD',
                    couponCode: 'TEST20'
                })
            );

            // Verify it does NOT contain PostObject
            const actualCallArgs = mockPost.mock.calls[0];
            const actualPayload = actualCallArgs[1];
            expect(actualPayload).not.toHaveProperty('PostObject');

            expect(result).toBe(mockResponse);
        });
    });
});
