import { useState, useCallback, useMemo } from 'react';

export interface PaginationState {
    currentPage: number;
    pageSize: number;
    totalItems: number;
}

export interface UsePaginationOptions {
    initialPage?: number;
    initialPageSize?: number;
    totalItems?: number;
}

export interface UsePaginationReturn {
    // State
    currentPage: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;

    // Computed
    startIndex: number;
    endIndex: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    isFirstPage: boolean;
    isLastPage: boolean;

    // Actions
    goToPage: (page: number) => void;
    nextPage: () => void;
    previousPage: () => void;
    firstPage: () => void;
    lastPage: () => void;
    setPageSize: (size: number) => void;
    setTotalItems: (total: number) => void;

    // For API
    getRequestParams: () => { pageIndex: number; pageSize: number };
}

/**
 * Hook for pagination state management
 */
export function usePagination(options: UsePaginationOptions = {}): UsePaginationReturn {
    const {
        initialPage = 1,
        initialPageSize = 10,
        totalItems: initialTotal = 0
    } = options;

    const [currentPage, setCurrentPage] = useState(initialPage);
    const [pageSize, setPageSizeState] = useState(initialPageSize);
    const [totalItems, setTotalItems] = useState(initialTotal);

    const totalPages = useMemo(() =>
        Math.max(1, Math.ceil(totalItems / pageSize)),
        [totalItems, pageSize]
    );

    const startIndex = useMemo(() =>
        (currentPage - 1) * pageSize,
        [currentPage, pageSize]
    );

    const endIndex = useMemo(() =>
        Math.min(startIndex + pageSize - 1, totalItems - 1),
        [startIndex, pageSize, totalItems]
    );

    const hasNextPage = currentPage < totalPages;
    const hasPreviousPage = currentPage > 1;
    const isFirstPage = currentPage === 1;
    const isLastPage = currentPage === totalPages;

    const goToPage = useCallback((page: number) => {
        const validPage = Math.max(1, Math.min(page, totalPages));
        setCurrentPage(validPage);
    }, [totalPages]);

    const nextPage = useCallback(() => {
        if (hasNextPage) {
            setCurrentPage(prev => prev + 1);
        }
    }, [hasNextPage]);

    const previousPage = useCallback(() => {
        if (hasPreviousPage) {
            setCurrentPage(prev => prev - 1);
        }
    }, [hasPreviousPage]);

    const firstPage = useCallback(() => setCurrentPage(1), []);

    const lastPage = useCallback(() => setCurrentPage(totalPages), [totalPages]);

    const setPageSize = useCallback((size: number) => {
        setPageSizeState(size);
        setCurrentPage(1); // Reset to first page when page size changes
    }, []);

    const getRequestParams = useCallback(() => ({
        pageIndex: currentPage - 1, // API usually uses 0-indexed
        pageSize,
    }), [currentPage, pageSize]);

    return {
        currentPage,
        pageSize,
        totalItems,
        totalPages,
        startIndex,
        endIndex,
        hasNextPage,
        hasPreviousPage,
        isFirstPage,
        isLastPage,
        goToPage,
        nextPage,
        previousPage,
        firstPage,
        lastPage,
        setPageSize,
        setTotalItems,
        getRequestParams,
    };
}
