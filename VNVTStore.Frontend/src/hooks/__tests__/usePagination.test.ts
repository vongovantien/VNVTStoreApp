import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { usePagination } from '../usePagination';

describe('usePagination', () => {
    describe('initialization', () => {
        it('should initialize with default values', () => {
            const { result } = renderHook(() => usePagination());

            expect(result.current.currentPage).toBe(1);
            expect(result.current.pageSize).toBe(10);
            expect(result.current.totalItems).toBe(0);
            expect(result.current.totalPages).toBe(1);
        });

        it('should initialize with custom values', () => {
            const { result } = renderHook(() =>
                usePagination({
                    initialPage: 3,
                    initialPageSize: 25,
                    totalItems: 100
                })
            );

            expect(result.current.currentPage).toBe(3);
            expect(result.current.pageSize).toBe(25);
            expect(result.current.totalItems).toBe(100);
            expect(result.current.totalPages).toBe(4);
        });
    });

    describe('computed values', () => {
        it('should calculate startIndex and endIndex correctly', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 2, initialPageSize: 10, totalItems: 50 })
            );

            expect(result.current.startIndex).toBe(10);
            expect(result.current.endIndex).toBe(19);
        });

        it('should calculate hasNextPage and hasPreviousPage', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 2, totalItems: 30 })
            );

            expect(result.current.hasNextPage).toBe(true);
            expect(result.current.hasPreviousPage).toBe(true);
        });

        it('should mark first page correctly', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 1, totalItems: 50 })
            );

            expect(result.current.isFirstPage).toBe(true);
            expect(result.current.hasPreviousPage).toBe(false);
        });

        it('should mark last page correctly', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 5, initialPageSize: 10, totalItems: 50 })
            );

            expect(result.current.isLastPage).toBe(true);
            expect(result.current.hasNextPage).toBe(false);
        });
    });

    describe('navigation', () => {
        it('should go to next page', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 1, totalItems: 50 })
            );

            act(() => {
                result.current.nextPage();
            });

            expect(result.current.currentPage).toBe(2);
        });

        it('should go to previous page', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 3, totalItems: 50 })
            );

            act(() => {
                result.current.previousPage();
            });

            expect(result.current.currentPage).toBe(2);
        });

        it('should go to specific page', () => {
            const { result } = renderHook(() =>
                usePagination({ totalItems: 50 })
            );

            act(() => {
                result.current.goToPage(4);
            });

            expect(result.current.currentPage).toBe(4);
        });

        it('should not exceed total pages', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPageSize: 10, totalItems: 30 })
            );

            act(() => {
                result.current.goToPage(10);
            });

            expect(result.current.currentPage).toBe(3); // max is 3
        });

        it('should not go below page 1', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 1, totalItems: 50 })
            );

            act(() => {
                result.current.previousPage();
            });

            expect(result.current.currentPage).toBe(1);
        });

        it('should go to first and last page', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 3, initialPageSize: 10, totalItems: 50 })
            );

            act(() => {
                result.current.lastPage();
            });
            expect(result.current.currentPage).toBe(5);

            act(() => {
                result.current.firstPage();
            });
            expect(result.current.currentPage).toBe(1);
        });
    });

    describe('page size', () => {
        it('should change page size and reset to first page', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 3, totalItems: 100 })
            );

            act(() => {
                result.current.setPageSize(25);
            });

            expect(result.current.pageSize).toBe(25);
            expect(result.current.currentPage).toBe(1); // reset to 1
            expect(result.current.totalPages).toBe(4);
        });
    });

    describe('API params', () => {
        it('should return 0-indexed page for API', () => {
            const { result } = renderHook(() =>
                usePagination({ initialPage: 3, initialPageSize: 20 })
            );

            const params = result.current.getRequestParams();

            expect(params.pageIndex).toBe(2); // 0-indexed
            expect(params.pageSize).toBe(20);
        });
    });
});
