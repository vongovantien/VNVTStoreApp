/**
 * Unit tests for useSEO hooks
 * Features: #85 SEO Head Management, #86 Canonical Tags, #87 Schema.org JSON-LD
 */
import { renderHook } from '@testing-library/react';
import { describe, it, expect, afterEach } from 'vitest';

import { useSEO, useCanonical, useProductSchema } from '@/hooks/useSEO';

describe('useSEO - Feature #85', () => {
    afterEach(() => {
        // Clean up head elements
        document.querySelectorAll('meta[name="description"]').forEach(el => el.remove());
        document.querySelectorAll('link[rel="canonical"]').forEach(el => el.remove());
        document.querySelectorAll('script[type="application/ld+json"]').forEach(el => el.remove());
    });

    it('sets document title with store suffix', () => {
        renderHook(() => useSEO({ title: 'Test Page' }));
        expect(document.title).toBe('Test Page | VNVT Store');
    });

    it('sets meta description when provided', () => {
        renderHook(() => useSEO({ title: 'Test', description: 'A test page description' }));
        const meta = document.querySelector('meta[name="description"]');
        expect(meta).toBeTruthy();
        expect(meta?.getAttribute('content')).toBe('A test page description');
    });

    it('restores previous title on unmount', () => {
        document.title = 'Original Title';
        const { unmount } = renderHook(() => useSEO({ title: 'New Title' }));
        expect(document.title).toBe('New Title | VNVT Store');
        unmount();
        expect(document.title).toBe('Original Title');
    });
});

describe('useCanonical - Feature #86', () => {
    afterEach(() => {
        document.querySelectorAll('link[rel="canonical"]').forEach(el => el.remove());
    });

    it('sets canonical link tag with path', () => {
        renderHook(() => useCanonical('/products/test'));
        const canonical = document.querySelector('link[rel="canonical"]');
        expect(canonical).toBeTruthy();
        // canonical href will be origin + path
        expect(canonical?.getAttribute('href')).toContain('/products/test');
    });

    it('sets canonical from current URL when no path', () => {
        renderHook(() => useCanonical());
        const canonical = document.querySelector('link[rel="canonical"]');
        expect(canonical).toBeTruthy();
        expect(canonical?.getAttribute('href')).toBeTruthy();
    });

    it('removes canonical on unmount', () => {
        const { unmount } = renderHook(() => useCanonical('/test'));
        expect(document.querySelector('link[rel="canonical"]')).toBeTruthy();
        unmount();
        expect(document.querySelector('link[rel="canonical"]')).toBeNull();
    });
});

describe('useProductSchema - Feature #87', () => {
    afterEach(() => {
        document.querySelectorAll('#product-jsonld').forEach(el => el.remove());
    });

    it('injects Schema.org JSON-LD for product', () => {
        renderHook(() => useProductSchema({
            name: 'iPhone 15 Pro',
            description: 'Latest Apple phone',
            price: 29990000,
            currency: 'VND',
            imageUrl: '/img/iphone.jpg',
            sku: 'IP15PRO',
            brand: 'Apple',
            availability: 'InStock',
        }));

        const script = document.getElementById('product-jsonld');
        expect(script).toBeTruthy();

        const data = JSON.parse(script!.textContent!);
        expect(data['@context']).toBe('https://schema.org');
        expect(data['@type']).toBe('Product');
        expect(data.name).toBe('iPhone 15 Pro');
        expect(data.offers.price).toBe(29990000);
    });

    it('includes aggregate rating when provided', () => {
        renderHook(() => useProductSchema({
            name: 'Test Product',
            description: 'Test',
            price: 100000,
            currency: 'VND',
            imageUrl: '/img/test.jpg',
            sku: 'TEST',
            brand: 'TestBrand',
            availability: 'InStock',
            rating: 4.5,
            reviewCount: 120,
        }));

        const script = document.getElementById('product-jsonld');
        const data = JSON.parse(script!.textContent!);
        expect(data.aggregateRating).toBeDefined();
        expect(data.aggregateRating.ratingValue).toBe(4.5);
        expect(data.aggregateRating.reviewCount).toBe(120);
    });

    it('does not inject schema when product is null', () => {
        renderHook(() => useProductSchema(null));
        const script = document.getElementById('product-jsonld');
        expect(script).toBeNull();
    });

    it('removes schema on unmount', () => {
        const { unmount } = renderHook(() => useProductSchema({
            name: 'Test',
            price: 100,
        }));
        expect(document.getElementById('product-jsonld')).toBeTruthy();
        unmount();
        expect(document.getElementById('product-jsonld')).toBeNull();
    });
});
