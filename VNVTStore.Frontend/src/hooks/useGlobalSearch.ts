import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { globalSearch, GlobalSearchResult } from '@/services/searchService';

/**
 * Hook tìm kiếm global với debounce 300ms
 */
export function useGlobalSearch(query: string, enabled: boolean = true) {
    const [debouncedQuery, setDebouncedQuery] = useState(query);

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedQuery(query);
        }, 300);

        return () => clearTimeout(timer);
    }, [query]);

    return useQuery<GlobalSearchResult>({
        queryKey: ['globalSearch', debouncedQuery],
        queryFn: () => globalSearch(debouncedQuery),
        enabled: enabled && debouncedQuery.length >= 2,
        staleTime: 30000, // Cache 30s
    });
}
