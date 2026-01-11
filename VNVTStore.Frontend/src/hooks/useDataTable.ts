import { useState, useMemo } from 'react';

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
        if (!searchQuery) return data;

        const lowerQuery = searchQuery.toLowerCase();

        return data.filter((item) => {
            if (filterFn) return filterFn(item, lowerQuery, searchField);

            // Default generic search
            if (searchField !== 'all') {
                const value = item[searchField];
                return String(value).toLowerCase().includes(lowerQuery);
            }

            // Search all fields
            return Object.values(item).some((val) =>
                String(val).toLowerCase().includes(lowerQuery)
            );
        });
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
        const newSelected = new Set(selectedIds);
        if (newSelected.has(id)) {
            newSelected.delete(id);
        } else {
            newSelected.add(id);
        }
        setSelectedIds(newSelected);
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
