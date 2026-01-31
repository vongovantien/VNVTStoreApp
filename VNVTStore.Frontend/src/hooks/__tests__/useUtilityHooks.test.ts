import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useDebounce, useLocalStorage, useClickOutside } from '../index';

describe('useDebounce', () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it('should return initial value immediately', () => {
        const { result } = renderHook(() => useDebounce('initial', 300));
        expect(result.current).toBe('initial');
    });

    it('should debounce value changes', async () => {
        const { result, rerender } = renderHook(
            ({ value }) => useDebounce(value, 300),
            { initialProps: { value: 'initial' } }
        );

        expect(result.current).toBe('initial');

        rerender({ value: 'updated' });

        // Value should not change immediately
        expect(result.current).toBe('initial');

        // Fast-forward time
        act(() => {
            vi.advanceTimersByTime(300);
        });

        expect(result.current).toBe('updated');
    });

    it('should reset timer on rapid changes', () => {
        const { result, rerender } = renderHook(
            ({ value }) => useDebounce(value, 300),
            { initialProps: { value: 'a' } }
        );

        rerender({ value: 'b' });
        act(() => {
            vi.advanceTimersByTime(100);
        });

        rerender({ value: 'c' });
        act(() => {
            vi.advanceTimersByTime(100);
        });

        rerender({ value: 'd' });
        act(() => {
            vi.advanceTimersByTime(300);
        });

        // Should be 'd' because previous timers were cancelled
        expect(result.current).toBe('d');
    });
});

describe('useLocalStorage', () => {
    beforeEach(() => {
        localStorage.clear();
    });

    it('should return initial value when no stored value', () => {
        const { result } = renderHook(() =>
            useLocalStorage('test-key', 'default')
        );

        expect(result.current[0]).toBe('default');
    });

    it('should return stored value if exists', () => {
        localStorage.setItem('test-key', JSON.stringify('stored'));

        const { result } = renderHook(() =>
            useLocalStorage('test-key', 'default')
        );

        expect(result.current[0]).toBe('stored');
    });

    it('should update localStorage when value changes', () => {
        const { result } = renderHook(() =>
            useLocalStorage('test-key', 'initial')
        );

        act(() => {
            result.current[1]('updated');
        });

        expect(result.current[0]).toBe('updated');
        expect(JSON.parse(localStorage.getItem('test-key')!)).toBe('updated');
    });

    it('should support function updates', () => {
        const { result } = renderHook(() =>
            useLocalStorage('test-key', 0)
        );

        act(() => {
            result.current[1]((prev) => prev + 1);
        });

        expect(result.current[0]).toBe(1);
    });

    it('should remove value from localStorage', () => {
        const { result } = renderHook(() =>
            useLocalStorage('test-key', 'initial')
        );

        act(() => {
            result.current[1]('stored');
        });

        act(() => {
            result.current[2](); // removeValue
        });

        expect(result.current[0]).toBe('initial');
        expect(localStorage.getItem('test-key')).toBeNull();
    });

    it('should handle objects', () => {
        const { result } = renderHook(() =>
            useLocalStorage('test-key', { name: 'test' })
        );

        act(() => {
            result.current[1]({ name: 'updated' });
        });

        expect(result.current[0]).toEqual({ name: 'updated' });
    });
});

describe('useClickOutside', () => {
    it('should return a ref', () => {
        const handler = vi.fn();
        const { result } = renderHook(() => useClickOutside<HTMLDivElement>(handler));

        expect(result.current).toBeDefined();
        expect(result.current.current).toBeNull();
    });

    it('should call handler when clicking outside', () => {
        const handler = vi.fn();
        const { result } = renderHook(() => useClickOutside<HTMLDivElement>(handler));

        // Create a mock element
        const element = document.createElement('div');
        document.body.appendChild(element);

        // Attach ref
        Object.defineProperty(result.current, 'current', {
            value: element,
            writable: true
        });

        // Simulate click outside
        const outsideElement = document.createElement('div');
        document.body.appendChild(outsideElement);

        const event = new MouseEvent('mousedown', { bubbles: true });
        Object.defineProperty(event, 'target', { value: outsideElement });
        document.dispatchEvent(event);

        expect(handler).toHaveBeenCalled();

        // Cleanup
        document.body.removeChild(element);
        document.body.removeChild(outsideElement);
    });
});
