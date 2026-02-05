import { useState, useCallback, useRef, useEffect, useMemo } from 'react';
import { debounce, throttle } from 'lodash-es';

// Re-export product hooks
export {
    useProducts,
    useProduct,
    useCreateProduct,
    useUpdateProduct,
    useDeleteProduct,
    useCategories,
    useCategoriesList,
    productKeys,
} from './useProducts';

export { useSuppliers, useSuppliersPaged } from './useSuppliers';
export { useUnits } from './useUnits';
export { useBrands } from './useBrands';

// Re-export order hooks
export { useOrders, useAdminOrders, useUpdateOrderStatus, useOrder } from './useOrders';

export * from './useEntityManager';
export * from './usePagination';
export * from './useRoles';
export { useDataTable } from './useDataTable';
export { useSignalR } from './useSignalR';

/**
 * Hook for debounced value
 * @param value - Value to debounce
 * @param delay - Debounce delay in ms
 */
export function useDebounce<T>(value: T, delay: number = 300): T {
    const [debouncedValue, setDebouncedValue] = useState<T>(value);

    useEffect(() => {
        const timer = setTimeout(() => setDebouncedValue(value), delay);
        return () => clearTimeout(timer);
    }, [value, delay]);

    return debouncedValue;
}

/**
 * Hook for debounced callback
 * @param callback - Callback to debounce
 * @param delay - Debounce delay in ms
 */
export function useDebouncedCallback<T extends (...args: unknown[]) => unknown>(
    callback: T,
    delay: number = 300
): T {
    const callbackRef = useRef(callback);

    useEffect(() => {
        callbackRef.current = callback;
    });

    const debouncedFn = useMemo(
        // eslint-disable-next-line react-hooks/refs
        () => debounce((...args: Parameters<T>) => callbackRef.current(...args), delay),
        [delay]
    );

    useEffect(() => {
        return () => {
            debouncedFn.cancel();
        };
    }, [debouncedFn]);

    return debouncedFn as unknown as T;
}

/**
 * Hook for throttled callback
 * @param callback - Callback to throttle
 * @param delay - Throttle delay in ms
 */
export function useThrottledCallback<T extends (...args: unknown[]) => unknown>(
    callback: T,
    delay: number = 300
): T {
    const callbackRef = useRef(callback);

    useEffect(() => {
        callbackRef.current = callback;
    });

    const throttledFn = useMemo(
        // eslint-disable-next-line react-hooks/refs
        () => throttle((...args: Parameters<T>) => callbackRef.current(...args), delay),
        [delay]
    );

    useEffect(() => {
        return () => {
            throttledFn.cancel();
        };
    }, [throttledFn]);

    return throttledFn as unknown as T;
}

/**
 * Hook for local storage state
 * @param key - Storage key
 * @param initialValue - Initial value
 */
export function useLocalStorage<T>(
    key: string,
    initialValue: T
): [T, (value: T | ((prev: T) => T)) => void, () => void] {
    const [storedValue, setStoredValue] = useState<T>(() => {
        try {
            const item = window.localStorage.getItem(key);
            return item ? JSON.parse(item) : initialValue;
        } catch {
            return initialValue;
        }
    });

    const setValue = useCallback(
        (value: T | ((prev: T) => T)) => {
            try {
                const valueToStore = value instanceof Function ? value(storedValue) : value;
                setStoredValue(valueToStore);
                window.localStorage.setItem(key, JSON.stringify(valueToStore));
            } catch (error) {
                console.error('Error saving to localStorage:', error);
            }
        },
        [key, storedValue]
    );

    const removeValue = useCallback(() => {
        try {
            window.localStorage.removeItem(key);
            setStoredValue(initialValue);
        } catch (error) {
            console.error('Error removing from localStorage:', error);
        }
    }, [key, initialValue]);

    return [storedValue, setValue, removeValue];
}

/**
 * Hook for click outside detection
 * @param handler - Handler to call when clicked outside
 */
export function useClickOutside<T extends HTMLElement>(
    handler: () => void
): React.RefObject<T | null> {
    const ref = useRef<T>(null);

    useEffect(() => {
        const listener = (event: MouseEvent | TouchEvent) => {
            if (!ref.current || ref.current.contains(event.target as Node)) {
                return;
            }
            handler();
        };

        document.addEventListener('mousedown', listener);
        document.addEventListener('touchstart', listener);

        return () => {
            document.removeEventListener('mousedown', listener);
            document.removeEventListener('touchstart', listener);
        };
    }, [handler]);

    return ref;
}

/**
 * Hook for window size
 */
export function useWindowSize() {
    const [size, setSize] = useState({
        width: typeof window !== 'undefined' ? window.innerWidth : 0,
        height: typeof window !== 'undefined' ? window.innerHeight : 0,
    });

    const handleResize = useThrottledCallback(() => {
        setSize({
            width: window.innerWidth,
            height: window.innerHeight,
        });
    }, 200);

    useEffect(() => {
        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, [handleResize]);

    return size;
}

/**
 * Hook for intersection observer (lazy loading)
 */
export function useIntersectionObserver(
    options?: IntersectionObserverInit
): [React.RefObject<HTMLDivElement | null>, boolean] {
    const ref = useRef<HTMLDivElement>(null);
    const [isIntersecting, setIsIntersecting] = useState(false);

    useEffect(() => {
        const observer = new IntersectionObserver(([entry]) => {
            setIsIntersecting(entry.isIntersecting);
        }, options);

        if (ref.current) {
            observer.observe(ref.current);
        }

        return () => observer.disconnect();
    }, [options]);

    return [ref, isIntersecting];
}

/**
 * Hook for previous value
 */
export function usePrevious<T>(value: T): T | undefined {
    const ref = useRef<T | undefined>(undefined);

    useEffect(() => {
        ref.current = value;
    }, [value]);

    // eslint-disable-next-line react-hooks/exhaustive-deps
    return useMemo(() => ref.current, [value]);
}
