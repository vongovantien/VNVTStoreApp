import { useState, useMemo } from 'react';
import { applyClientFilters } from '@/utils/queryHelper';
import { SearchCondition } from '@/services/api';

interface UseDataTableProps<T> {
    data: T[];
    filterFn?: (item: T, filterTerm: string, filterField?: string) => boolean;
    initialSort?: { field: keyof T; direction: 'asc' | 'desc' };
}

export const useDataTable = <T extends { id: string | number } & Record<string, any>>({
    data,
    filterFn,
    initialSort,
}: UseDataTableProps<T>) => {
    // Selection
    const [selectedIds, setSelectedIds] = useState<Set<string | number>>(new Set());

    // Search & Filter
    const [searchQuery, setSearchQuery] = useState('');
    const [searchField, setSearchField] = useState<string>('all');

    // Sort
    const [sortConfig, setSortConfig] = useState<{ field: keyof T; direction: 'asc' | 'desc' } | null>(
        initialSort || null
    );

    // Pagination
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);

    // --- Logic ---

    // 1. Filter
    const filteredData = useMemo(() => {
        let result = data;

        // Apply external filters (complex SearchDTOs) logic locally if data is all here
        // Note: If server-side pagination is used, 'data' is usually just current page, so this runs on that subset.
        // But for client-side tables, this is powerful.
        // We can extend this hook to accept 'filters' prop if needed, 
        // but for now let's keep searchField/searchQuery behavior and map it to a SearchDTO.

        if (searchQuery) {
            const lowerQuery = searchQuery.toLowerCase();
            // Reuse the simple logic OR use the new helper?
            // Let's use the new helper for single field search to ensure consistency
            if (searchField !== 'all') {
                result = applyClientFilters(result, [{
                    searchField: searchField,
                    searchCondition: SearchCondition.Contains,
                    searchValue: lowerQuery
                }]);
            } else {
                // Fallback to "All Fields" search which is not in SearchDTO standard usually
                result = result.filter(item =>
                    Object.values(item).some(val =>
                        String(val).toLowerCase().includes(lowerQuery)
                    )
                );
            }
        }

        if (filterFn) {
            result = result.filter(item => filterFn(item, searchQuery.toLowerCase(), searchField));
        }

        return result;
    }, [data, searchQuery, searchField, filterFn]);

    // 2. Sort
    const sortedData = useMemo(() => {
        if (!sortConfig) return filteredData;

        return [...filteredData].sort((a, b) => {
            const aVal = a[sortConfig.field];
            const bVal = b[sortConfig.field];

            if (aVal < bVal) return sortConfig.direction === 'asc' ? -1 : 1;
            if (aVal > bVal) return sortConfig.direction === 'asc' ? 1 : -1;
            return 0;
        });
    }, [filteredData, sortConfig]);

    // 3. Paginate
    const paginatedData = useMemo(() => {
        const startIndex = (currentPage - 1) * itemsPerPage;
        return sortedData.slice(startIndex, startIndex + itemsPerPage);
    }, [sortedData, currentPage, itemsPerPage]);

    const totalPages = Math.ceil(filteredData.length / itemsPerPage);

    // Handlers
    const handleSelectAll = (checked: boolean) => {
        if (checked) {
            // Select all visible items (or all filtered items depending on UX preference)
            // Usually users expect "Select All" to select current page or all 
            // Let's implement select all filtered for power usage
            setSelectedIds(new Set(filteredData.map(d => d.id)));
        } else {
            setSelectedIds(new Set());
        }
    };

    const handleSelectRow = (id: string | number) => {
        setSelectedIds((prev) => {
            const next = new Set(prev);
            if (next.has(id)) {
                next.delete(id);
            } else {
                next.add(id);
            }
            return next;
        });
    };

    const handleSort = (field: keyof T) => {
        setSortConfig((current) => {
            if (current?.field === field) {
                return { field, direction: current.direction === 'asc' ? 'desc' : 'asc' };
            }
            return { field, direction: 'asc' };
        });
    };

    return {
        // State
        selectedIds,
        searchQuery,
        searchField,
        sortConfig,
        currentPage,
        itemsPerPage,

        // Data
        data: paginatedData,
        totalItems: filteredData.length,
        totalPages,

        // Setters
        setSearchQuery,
        setSearchField,
        setItemsPerPage,
        setCurrentPage,

        // Actions
        handleSelectAll,
        handleSelectRow,
        handleSort,
        resetSelection: () => setSelectedIds(new Set()),
    };
};
