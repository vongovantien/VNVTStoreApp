import { useState, useCallback, useRef, useEffect, forwardRef, useMemo } from 'react';
import { ChevronDown, Search, Loader2, Check } from 'lucide-react';
import { cn } from '@/utils/cn';
import { useInfiniteQuery } from '@tanstack/react-query';
import { useDebounce } from '@/hooks';

export interface LazySelectOption {
    value: string;
    label: string;
}

interface LazySelectProps {
    /** Current value */
    value?: string;
    /** Callback when value changes */
    onChange?: (value: string) => void;
    /** Placeholder text */
    placeholder?: string;
    /** Label */
    label?: string;
    /** Error message */
    error?: string;
    /** Is required */
    required?: boolean;
    /** Is disabled */
    disabled?: boolean;
    /** Full width */
    fullWidth?: boolean;
    /** Custom className */
    className?: string;
    /** Query key prefix */
    queryKeyPrefix: string;
    /** Fetch function - must return { items, hasMore } */
    fetchFn: (params: { pageIndex: number; pageSize: number; search?: string }) => Promise<{
        items: LazySelectOption[];
        totalItems: number;
    }>;
    /** Page size for pagination */
    initialLabel?: string;
    /** Page size for pagination */
    pageSize?: number;
    /** ID for the button element */
    id?: string;
}

export const LazySelect = forwardRef<HTMLDivElement, LazySelectProps>(
    (
        {
            value,
            onChange,
            placeholder = 'Chọn...',
            label,
            error,
            required,
            disabled,
            fullWidth = true,
            className,
            queryKeyPrefix,
            fetchFn,
            pageSize = 20,
            initialLabel,
            id,
        },
        ref
    ) => {
        const [isOpen, setIsOpen] = useState(false);
        const [searchTerm, setSearchTerm] = useState('');
        const debouncedSearch = useDebounce(searchTerm, 800);
        const containerRef = useRef<HTMLDivElement>(null);
        const listRef = useRef<HTMLDivElement>(null);

        // Infinite query for lazy loading
        const {
            data,
            fetchNextPage,
            hasNextPage,
            isFetchingNextPage,
            isLoading,
        } = useInfiniteQuery({
            queryKey: [queryKeyPrefix, 'lazy-select', debouncedSearch],
            queryFn: async ({ pageParam = 1 }) => {
                const result = await fetchFn({
                    pageIndex: pageParam,
                    pageSize,
                    search: debouncedSearch || undefined,
                });
                return {
                    items: result.items,
                    nextPage: result.items.length === pageSize ? pageParam + 1 : undefined,
                    totalItems: result.totalItems,
                };
            },
            getNextPageParam: (lastPage) => lastPage.nextPage,
            initialPageParam: 1,
            enabled: isOpen,
        });

        // Flatten all pages into single array
        const options = useMemo(() => {
            if (!data?.pages) return [];
            return data.pages.flatMap((page) => page.items);
        }, [data?.pages]);

        // Find selected option label
        const selectedLabel = useMemo(() => {
            const found = options.find((opt) => opt.value === value);
            return found?.label || initialLabel || (value ? value : '');
        }, [options, value, initialLabel]);

        // Handle click outside to close
        useEffect(() => {
            const handleClickOutside = (e: MouseEvent) => {
                if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
                    setIsOpen(false);
                }
            };
            document.addEventListener('mousedown', handleClickOutside);
            return () => document.removeEventListener('mousedown', handleClickOutside);
        }, []);

        // Handle scroll for infinite loading
        const handleScroll = useCallback(() => {
            if (!listRef.current || isFetchingNextPage || !hasNextPage) return;

            const { scrollTop, scrollHeight, clientHeight } = listRef.current;
            if (scrollTop + clientHeight >= scrollHeight - 50) {
                fetchNextPage();
            }
        }, [fetchNextPage, hasNextPage, isFetchingNextPage]);

        // Handle option select
        const handleSelect = useCallback(
            (optionValue: string) => {
                onChange?.(optionValue);
                setIsOpen(false);
                setSearchTerm('');
            },
            [onChange]
        );

        const inputId = id || `lazy-select-${queryKeyPrefix}-${Math.random().toString(36).substr(2, 9)}`;

        return (
            <div
                ref={ref}
                className={cn('flex flex-col gap-1', fullWidth && 'w-full', className)}
            >
                {/* Label */}
                {label && (
                    <label htmlFor={inputId} className="font-bold text-primary text-sm">
                        {label}
                        {required && <span className="text-red-500 ml-1">*</span>}
                    </label>
                )}

                {/* Select Container */}
                <div ref={containerRef} className="relative">
                    {/* Trigger Button */}
                    <button
                        type="button"
                        id={inputId}
                        disabled={disabled}
                        onClick={() => !disabled && setIsOpen(!isOpen)}
                        className={cn(
                            'w-full px-4 py-2 text-sm text-left bg-primary border rounded-lg',
                            'flex items-center justify-between gap-2',
                            'transition-all duration-200 outline-none',
                            'focus:border-primary focus:ring-1 focus:ring-primary',
                            'disabled:opacity-50 disabled:cursor-not-allowed',
                            error ? 'border-error' : 'border-gray-200',
                            isOpen && 'border-primary ring-1 ring-primary'
                        )}
                    >
                        <span className={cn(!value && 'text-tertiary')}>
                            {selectedLabel || placeholder}
                        </span>
                        <ChevronDown
                            size={16}
                            className={cn(
                                'text-tertiary transition-transform',
                                isOpen && 'rotate-180'
                            )}
                        />
                    </button>

                    {/* Dropdown */}
                    {isOpen && (
                        <div className="absolute z-50 w-full mt-1 bg-primary border border-gray-200 rounded-lg shadow-lg overflow-hidden">
                            {/* Search Input */}
                            <div className="p-2 border-b border-gray-100">
                                <div className="relative">
                                    <Search
                                        size={16}
                                        className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary"
                                    />
                                    <input
                                        type="text"
                                        value={searchTerm}
                                        onChange={(e) => setSearchTerm(e.target.value)}
                                        placeholder="Tìm kiếm..."
                                        className="w-full pl-9 pr-3 py-2 text-sm bg-slate-50 dark:bg-slate-900/50 border border-gray-200 rounded-lg outline-none focus:border-primary"
                                        autoFocus
                                    />
                                </div>
                            </div>

                            {/* Options List */}
                            <div
                                ref={listRef}
                                onScroll={handleScroll}
                                className="max-h-60 overflow-y-auto"
                            >
                                {isLoading ? (
                                    <div className="flex flex-col items-center justify-center py-6 text-tertiary gap-2">
                                        <Loader2 size={24} className="animate-spin text-primary" />
                                        <span className="text-xs">Đang tải...</span>
                                    </div>
                                ) : options.length === 0 ? (
                                    <div className="py-4 text-center text-sm text-tertiary">
                                        Không tìm thấy kết quả
                                    </div>
                                ) : (
                                    <>
                                        {options.map((option) => (
                                            <button
                                                key={option.value}
                                                type="button"
                                                onClick={() => handleSelect(option.value)}
                                                className={cn(
                                                    'w-full px-4 py-2 text-sm text-left flex items-center gap-2',
                                                    'hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors',
                                                    value === option.value && 'bg-accent-primary/10 text-accent-primary'
                                                )}
                                            >
                                                {value === option.value && <Check size={14} />}
                                                <span className={cn(value !== option.value && 'ml-5')}>
                                                    {option.label}
                                                </span>
                                            </button>
                                        ))}

                                        {/* Loading more indicator */}
                                        {isFetchingNextPage && (
                                            <div className="flex items-center justify-center py-2">
                                                <Loader2 size={16} className="animate-spin text-tertiary" />
                                            </div>
                                        )}
                                    </>
                                )}
                            </div>
                        </div>
                    )}
                </div>

                {/* Error message */}
                {error && <span className="text-xs text-error">{error}</span>}
            </div>
        );
    }
);

LazySelect.displayName = 'LazySelect';

export default LazySelect;
