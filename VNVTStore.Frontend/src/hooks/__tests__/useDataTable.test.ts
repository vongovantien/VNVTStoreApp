import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useDataTable } from '../useDataTable';
import { PaginationDefaults, SortDirection } from '@/constants';

describe('useDataTable', () => {
    describe('initialization', () => {
        it('should initialize with default values', () => {
            const { result } = renderHook(() => useDataTable());

            expect(result.current.currentPage).toBe(PaginationDefaults.PAGE_INDEX);
            expect(result.current.pageSize).toBe(PaginationDefaults.PAGE_SIZE);
            expect(result.current.searchQuery).toBe('');
            expect(result.current.selectedIds.size).toBe(0);
        });

        it('should initialize with custom values', () => {
            const { result } = renderHook(() =>
                useDataTable({
                    defaultPageIndex: 2,
                    defaultPageSize: 20,
                    defaultSortField: 'price',
                    defaultSortDir: SortDirection.ASC
                })
            );

            expect(result.current.currentPage).toBe(2);
            expect(result.current.pageSize).toBe(20);
            expect(result.current.sortField).toBe('price');
            expect(result.current.sortDir).toBe(SortDirection.ASC);
        });
    });

    describe('state updates', () => {
        it('should update search query', () => {
            const { result } = renderHook(() => useDataTable({ defaultPageIndex: 2 }));

            act(() => {
                result.current.setSearchQuery('test');
            });

            expect(result.current.searchQuery).toBe('test');
        });

        it('should update pagination', () => {
            const { result } = renderHook(() => useDataTable());

            act(() => {
                result.current.setCurrentPage(3);
                result.current.setPageSize(50);
            });

            expect(result.current.currentPage).toBe(3);
            expect(result.current.pageSize).toBe(50);
        });

        it('should update sorting', () => {
            const { result } = renderHook(() => useDataTable());

            act(() => {
                result.current.onSort('name', 'asc');
            });

            expect(result.current.sortField).toBe('name');
            expect(result.current.sortDir).toBe('asc');
            expect(result.current.currentPage).toBe(1); // onSort resets page
        });
    });

    describe('selection', () => {
        it('should update selection', () => {
            const { result } = renderHook(() => useDataTable());

            const ids = new Set(['1', '2']);
            act(() => {
                result.current.setSelectedIds(ids);
            });

            expect(result.current.selectedIds).toEqual(ids);
        });

        it('should reset selection', () => {
            const { result } = renderHook(() => useDataTable());

            act(() => {
                result.current.setSelectedIds(new Set(['1']));
                result.current.resetSelection();
            });

            expect(result.current.selectedIds.size).toBe(0);
        });
    });

    describe('reset', () => {
        it('should reset filters and pagination', () => {
            const { result } = renderHook(() =>
                useDataTable({ defaultPageIndex: 1, defaultPageSize: 10 })
            );

            act(() => {
                result.current.setSearchQuery('test');
                result.current.setCurrentPage(5);
                result.current.setFilters({ status: 'active' });
                result.current.resetFilters();
            });

            expect(result.current.searchQuery).toBe('');
            expect(result.current.currentPage).toBe(1);
            expect(result.current.filters).toEqual({});
        });
    });
});
