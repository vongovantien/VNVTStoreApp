import { useState, useCallback, useEffect } from 'react';
import { PaginationDefaults, SortDirection } from '@/constants';

// Custom debounce hook for search
function useDebouncedValue<T>(value: T, delay: number = 300): T {
    const [debouncedValue, setDebouncedValue] = useState(value);

    useEffect(() => {
        const timer = setTimeout(() => setDebouncedValue(value), delay);
        return () => clearTimeout(timer);
    }, [value, delay]);

    return debouncedValue;
}

export interface UseDataTableProps {
    defaultPageIndex?: number;
    defaultPageSize?: number;
    defaultSortField?: string;
    defaultSortDir?: SortDirection;
    initialFilters?: Record<string, string>;
    searchDebounceMs?: number;
}

export interface UseDataTableReturn {
    // Pagination
    currentPage: number;
    pageSize: number;
    setCurrentPage: (page: number) => void;
    setPageSize: (size: number) => void;

    // Sorting
    sortField: string;
    sortDir: SortDirection;
    onSort: (field: string, dir: 'asc' | 'desc') => void;

    // Search & Filter
    searchQuery: string;
    debouncedSearchQuery: string;
    setSearchQuery: (query: string) => void;
    filters: Record<string, string>;
    setFilters: (filters: Record<string, string>) => void;
    handleAdvancedSearch: (filters: Record<string, string>) => void;
    resetFilters: () => void;

    // Selection
    selectedIds: Set<string>;
    setSelectedIds: (ids: Set<string>) => void;
    resetSelection: () => void;
}

export function useDataTable({
    defaultPageIndex = PaginationDefaults.PAGE_INDEX,
    defaultPageSize = PaginationDefaults.PAGE_SIZE,
    defaultSortField = 'createdAt',
    defaultSortDir = SortDirection.DESC,
    initialFilters = {},
    searchDebounceMs = 300
}: UseDataTableProps = {}): UseDataTableReturn {

    // Pagination State
    const [currentPage, setCurrentPage] = useState(defaultPageIndex);
    const [pageSize, setPageSize] = useState(defaultPageSize);

    // Sorting State
    const [sortField, setSortField] = useState(defaultSortField);
    const [sortDir, setSortDir] = useState(defaultSortDir);

    // Search & Filter State
    const [searchQuery, setSearchQuery] = useState('');
    const debouncedSearchQuery = useDebouncedValue(searchQuery, searchDebounceMs);
    const [filters, setFilters] = useState<Record<string, string>>(initialFilters);

    // Selection State
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

    // Handlers
    const onSort = useCallback((field: string, dir: 'asc' | 'desc') => {
        setSortField(field);
        setSortDir(dir as SortDirection);
        setCurrentPage(defaultPageIndex); // Reset to first page on sort change
    }, [defaultPageIndex]);

    const handleAdvancedSearch = useCallback((newFilters: Record<string, string>) => {
        setFilters(newFilters);
        // If there's a specific 'search' field in the advanced filters, sync it
        if (newFilters.search !== undefined) {
            setSearchQuery(newFilters.search);
        }
        setCurrentPage(defaultPageIndex);
    }, [defaultPageIndex]);

    const resetFilters = useCallback(() => {
        setFilters(initialFilters);
        setSearchQuery('');
        setCurrentPage(defaultPageIndex);
        setPageSize(defaultPageSize);
        setSortField(defaultSortField);
        setSortDir(defaultSortDir);
    }, [defaultPageIndex, defaultPageSize, defaultSortField, defaultSortDir, initialFilters]);

    const resetSelection = useCallback(() => {
        setSelectedIds(new Set());
    }, []);

    return {
        currentPage,
        pageSize,
        setCurrentPage,
        setPageSize,
        sortField,
        sortDir,
        onSort,
        searchQuery,
        debouncedSearchQuery,
        setSearchQuery,
        filters,
        setFilters,
        handleAdvancedSearch,
        resetFilters,
        selectedIds,
        setSelectedIds,
        resetSelection
    };
}
