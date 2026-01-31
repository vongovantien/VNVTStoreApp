import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useDataTable } from '../useDataTable';

interface TestItem {
    id: string;
    name: string;
    price: number;
    category: string;
}

const mockData: TestItem[] = [
    { id: '1', name: 'Apple', price: 100, category: 'Fruit' },
    { id: '2', name: 'Banana', price: 50, category: 'Fruit' },
    { id: '3', name: 'Carrot', price: 30, category: 'Vegetable' },
    { id: '4', name: 'Donut', price: 25, category: 'Bakery' },
    { id: '5', name: 'Egg', price: 15, category: 'Dairy' },
];

describe('useDataTable', () => {
    describe('initialization', () => {
        it('should initialize with data', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            expect(result.current.data.length).toBeGreaterThan(0);
            expect(result.current.totalItems).toBe(5);
            expect(result.current.currentPage).toBe(1);
        });

        it('should start with empty selection', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            expect(result.current.selectedIds.size).toBe(0);
        });
    });

    describe('search', () => {
        it('should filter data by search query', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.setSearchQuery('apple');
            });

            expect(result.current.totalItems).toBe(1);
        });

        it('should be case insensitive', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.setSearchQuery('BANANA');
            });

            expect(result.current.totalItems).toBe(1);
        });

        it('should filter by specific field', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.setSearchField('category');
                result.current.setSearchQuery('Fruit');
            });

            expect(result.current.totalItems).toBe(2);
        });
    });

    describe('sorting', () => {
        it('should sort ascending by field', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.handleSort('name');
            });

            expect(result.current.sortConfig?.field).toBe('name');
            expect(result.current.sortConfig?.direction).toBe('asc');
        });

        it('should toggle sort direction', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.handleSort('price');
            });
            expect(result.current.sortConfig?.direction).toBe('asc');

            act(() => {
                result.current.handleSort('price');
            });
            expect(result.current.sortConfig?.direction).toBe('desc');
        });
    });

    describe('selection', () => {
        it('should select a row', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.handleSelectRow('1');
            });

            expect(result.current.selectedIds.has('1')).toBe(true);
        });

        it('should deselect a row', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.handleSelectRow('1');
                result.current.handleSelectRow('1');
            });

            expect(result.current.selectedIds.has('1')).toBe(false);
        });

        it('should select all rows', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.handleSelectAll(true);
            });

            expect(result.current.selectedIds.size).toBe(5);
        });

        it('should deselect all rows', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.handleSelectAll(true);
                result.current.handleSelectAll(false);
            });

            expect(result.current.selectedIds.size).toBe(0);
        });

        it('should reset selection', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: mockData })
            );

            act(() => {
                result.current.handleSelectRow('1');
                result.current.handleSelectRow('2');
                result.current.resetSelection();
            });

            expect(result.current.selectedIds.size).toBe(0);
        });
    });

    describe('pagination', () => {
        const largeData = Array.from({ length: 25 }, (_, i) => ({
            id: String(i + 1),
            name: `Item ${i + 1}`,
            price: (i + 1) * 10,
            category: 'Test',
        }));

        it('should paginate data', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: largeData })
            );

            expect(result.current.data.length).toBe(10); // default page size
            expect(result.current.totalPages).toBe(3);
        });

        it('should navigate pages', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: largeData })
            );

            act(() => {
                result.current.setCurrentPage(2);
            });

            expect(result.current.currentPage).toBe(2);
        });

        it('should change items per page', () => {
            const { result } = renderHook(() =>
                useDataTable({ data: largeData })
            );

            act(() => {
                result.current.setItemsPerPage(25);
            });

            expect(result.current.data.length).toBe(25);
            expect(result.current.totalPages).toBe(1);
        });
    });
});
