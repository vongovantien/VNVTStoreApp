import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useEntityManager, EntityService } from '../useEntityManager';

// Mock toast
vi.mock('@/store', () => ({
    useToast: () => ({
        success: vi.fn(),
        error: vi.fn(),
    }),
}));

// Mock i18n
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string) => key,
    }),
}));

interface TestEntity {
    code: string;
    name: string;
}

const createTestService = (): EntityService<TestEntity, Partial<TestEntity>, Partial<TestEntity>> => ({
    create: vi.fn().mockResolvedValue({ success: true, data: { code: '1', name: 'Created' } }),
    update: vi.fn().mockResolvedValue({ success: true, data: { code: '1', name: 'Updated' } }),
    delete: vi.fn().mockResolvedValue({ success: true }),
    getByCode: vi.fn().mockResolvedValue({ success: true, data: { code: '1', name: 'Fetched' } }),
});

const wrapper = ({ children }: { children: React.ReactNode }) => {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });
    return React.createElement(QueryClientProvider, { client: queryClient }, children);
};

describe('useEntityManager', () => {
    let mockService: EntityService<TestEntity, Partial<TestEntity>, Partial<TestEntity>>;

    beforeEach(() => {
        mockService = createTestService();
        vi.clearAllMocks();
    });

    it('should initialize with closed form and no editing item', () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        expect(result.current.isFormOpen).toBe(false);
        expect(result.current.editingItem).toBeNull();
        expect(result.current.itemToDelete).toBeNull();
    });

    it('should open form for create', () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        act(() => {
            result.current.openCreate();
        });

        expect(result.current.isFormOpen).toBe(true);
        expect(result.current.editingItem).toBeNull();
    });

    it('should open form for edit with item', async () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        const testItem: TestEntity = { code: '1', name: 'Test' };

        await act(async () => {
            await result.current.openEdit(testItem);
        });

        expect(result.current.isFormOpen).toBe(true);
    });

    it('should close form and reset editing item', () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        act(() => {
            result.current.openCreate();
        });

        act(() => {
            result.current.closeForm();
        });

        expect(result.current.isFormOpen).toBe(false);
        expect(result.current.editingItem).toBeNull();
    });

    it('should set item to delete when confirmDelete is called', () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        const testItem: TestEntity = { code: '1', name: 'Test' };

        act(() => {
            result.current.confirmDelete(testItem);
        });

        expect(result.current.itemToDelete).toEqual(testItem);
    });

    it('should clear itemToDelete when cancelDelete is called', () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        const testItem: TestEntity = { code: '1', name: 'Test' };

        act(() => {
            result.current.confirmDelete(testItem);
        });

        act(() => {
            result.current.cancelDelete();
        });

        expect(result.current.itemToDelete).toBeNull();
    });

    it('should call service.create when create is called', async () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        const createData = { name: 'New Item' };

        act(() => {
            result.current.create(createData);
        });

        await waitFor(() => {
            expect(mockService.create).toHaveBeenCalledWith(createData);
        });
    });

    it('should have correct loading states', () => {
        const { result } = renderHook(
            () => useEntityManager({
                service: mockService,
                queryKey: ['test'],
            }),
            { wrapper }
        );

        expect(result.current.isLoading).toBe(false);
        expect(result.current.isCreating).toBe(false);
        expect(result.current.isUpdating).toBe(false);
        expect(result.current.isDeleting).toBe(false);
    });
});
