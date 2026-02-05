import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useRoles, useRole } from '../useRoles';
import { roleService } from '../../services/roleService';

// Mock roleService
vi.mock('../../services/roleService', () => ({
    roleService: {
        search: vi.fn(),
        getByCode: vi.fn(),
    },
}));

const wrapper = ({ children }: { children: React.ReactNode }) => {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
        },
    });
    return React.createElement(QueryClientProvider, { client: queryClient }, children);
};

describe('useRoles hooks', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe('useRoles', () => {
        it('should fetch roles with params', async () => {
            const mockData = {
                success: true,
                message: '',
                statusCode: 200,
                data: {
                    items: [{ code: 'admin', name: 'Admin' }],
                    totalItems: 1,
                    pageIndex: 1,
                    pageSize: 10,
                    totalPages: 1,
                    hasPreviousPage: false,
                    hasNextPage: false
                }
            };
            vi.mocked(roleService.search).mockResolvedValue(mockData);

            const params = { pageIndex: 1, pageSize: 10 };
            const { result } = renderHook(() => useRoles(params), { wrapper });

            await waitFor(() => expect(result.current.isSuccess).toBe(true));
            expect(result.current.data).toEqual(mockData);
            expect(roleService.search).toHaveBeenCalledWith(params);
        });

        it('should handle error states', async () => {
            vi.mocked(roleService.search).mockRejectedValue(new Error('Fetch failed'));

            const { result } = renderHook(() => useRoles({}), { wrapper });

            await waitFor(() => expect(result.current.isError).toBe(true));
            expect(result.current.error).toBeDefined();
        });
    });

    describe('useRole', () => {
        it('should fetch a single role by code', async () => {
            const mockRole = { code: 'admin', name: 'Admin', permissions: [] };
            const mockResponse = { success: true, message: '', statusCode: 200, data: mockRole };
            vi.mocked(roleService.getByCode).mockResolvedValue(mockResponse);

            const { result } = renderHook(() => useRole('admin'), { wrapper });

            await waitFor(() => expect(result.current.isSuccess).toBe(true));
            expect(result.current.data).toEqual(mockResponse);
            expect(roleService.getByCode).toHaveBeenCalledWith('admin');
        });

        it('should not fetch if code is empty', () => {
            renderHook(() => useRole(''), { wrapper });
            expect(roleService.getByCode).not.toHaveBeenCalled();
        });
    });
});
