import { describe, it, expect } from 'vitest';
import { formatCurrency, formatNumber, slugify, getStatusColor, calculateDiscount, formatFileSize, formatRelativeTime } from './format';

describe('formatCurrency', () => {
    it('formats positive integers', () => {
        expect(formatCurrency(1000)).toMatch(/1\.000|1,000/); // Locale dependent safe check
        expect(formatCurrency(1000)).toContain('₫');
    });

    it('formats zero', () => {
        expect(formatCurrency(0)).toMatch(/0/);
    });

    it('formats negative numbers', () => {
        expect(formatCurrency(-50000)).toContain('-');
    });

    it('formats large numbers', () => {
        expect(formatCurrency(1000000000)).toMatch(/1\.000\.000\.000|1,000,000,000/);
    });
});

describe('slugify - Strict Vietnamese Support', () => {
    const cases = [
        ['Chào bạn', 'chao-ban'],
        ['Đường đời', 'duong-doi'],
        ['Học Lập Trình', 'hoc-lap-trinh'],
        ['Mắt Biếc & Tôi', 'mat-biec-toi'],
        ['   Trim Spaces   ', 'trim-spaces'],
        ['---Multiple---Dashes---', 'multiple-dashes'],
    ];

    it.each(cases)('should convert "%s" to "%s"', (input, expected) => {
        expect(slugify(input)).toBe(expected);
    });
});

describe('calculateDiscount', () => {
    it('calculates correct percentage', () => {
        expect(calculateDiscount(100, 80)).toBe(20);
        expect(calculateDiscount(1000, 500)).toBe(50);
    });

    it('handles zero original price', () => {
        expect(calculateDiscount(0, 100)).toBe(0);
    });

    it('handles price higher than original (negative discount)', () => {
        expect(calculateDiscount(100, 120)).toBe(-20);
    });

    it('rounds correctly', () => {
        expect(calculateDiscount(100, 66)).toBe(34); // 34%
        expect(calculateDiscount(100, 66.6)).toBe(33); // 33.4 => 33 round? or floor? Code uses Math.round
        // (100-66.6)/100 = 0.334 * 100 = 33.4 -> 33
    });
});

describe('formatFileSize', () => {
    it('formats bytes', () => {
        expect(formatFileSize(0)).toBe('0 B');
        expect(formatFileSize(500)).toBe('500 B');
    });

    it('formats KB', () => {
        expect(formatFileSize(1024)).toBe('1 KB');
        expect(formatFileSize(1536)).toBe('1.5 KB');
    });

    it('formats MB', () => {
        expect(formatFileSize(1024 * 1024)).toBe('1 MB');
    });

    it('formats GB', () => {
        expect(formatFileSize(1024 * 1024 * 1024)).toBe('1 GB');
    });
});

describe('getStatusColor', () => {
    it('returns valid colors', () => {
        const knownStatuses = ['pending', 'confirmed', 'shipping', 'delivered', 'cancelled'];
        knownStatuses.forEach(status => {
            expect(['warning', 'secondary', 'primary', 'success', 'error', 'info', 'default']).toContain(getStatusColor(status));
        });
    });
});
