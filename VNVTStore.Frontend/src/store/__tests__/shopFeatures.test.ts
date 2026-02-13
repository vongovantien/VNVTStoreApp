import { describe, it, expect, beforeEach } from 'vitest';
import { useCompareStore, usePriceAlertStore } from '../index';

const mockProduct = (code: string) => ({
    code,
    name: `Product ${code}`,
    price: 1000,
    category: 'Test',
    categoryCode: 'C1',
    image: '',
    description: '',
    stockQuantity: 10,
    averageRating: 4,
    reviewCount: 5,
    createdAt: new Date().toISOString()
});

describe('useCompareStore', () => {
    beforeEach(() => {
        useCompareStore.getState().clearCompare();
    });

    it('adds items to compare (respecting maxItems)', () => {
        const store = useCompareStore.getState();
        const p1 = mockProduct('P1');
        const p2 = mockProduct('P2');
        const p3 = mockProduct('P3');
        const p4 = mockProduct('P4');

        store.addItem(p1);
        store.addItem(p2);
        store.addItem(p3);

        expect(useCompareStore.getState().items).toHaveLength(3);
        expect(useCompareStore.getState().items[0].code).toBe('P1');

        // Adding 4th item - should push out the oldest (P1)
        useCompareStore.getState().addItem(p4);
        expect(useCompareStore.getState().items).toHaveLength(3);
        expect(useCompareStore.getState().items[0].code).toBe('P2');
        expect(useCompareStore.getState().items[2].code).toBe('P4');
    });

    it('removes items from compare', () => {
        const store = useCompareStore.getState();
        store.addItem(mockProduct('P1'));
        store.addItem(mockProduct('P2'));

        store.removeItem('P1');
        expect(useCompareStore.getState().items).toHaveLength(1);
        expect(useCompareStore.getState().items[0].code).toBe('P2');
    });

    it('checks if item is in compare', () => {
        const store = useCompareStore.getState();
        store.addItem(mockProduct('P1'));

        expect(useCompareStore.getState().isInCompare('P1')).toBe(true);
        expect(useCompareStore.getState().isInCompare('P2')).toBe(false);
    });
});

describe('usePriceAlertStore', () => {
    beforeEach(() => {
        // Reset watchlist manually if needed, or assume clean state
        const state = usePriceAlertStore.getState();
        state.watchlist.forEach(code => state.toggleAlert(code));
    });

    it('toggles alerts correctly', () => {
        const store = usePriceAlertStore.getState();
        const code = 'PROD123';

        expect(store.isWatched(code)).toBe(false);

        store.toggleAlert(code);
        expect(usePriceAlertStore.getState().isWatched(code)).toBe(true);

        usePriceAlertStore.getState().toggleAlert(code);
        expect(usePriceAlertStore.getState().isWatched(code)).toBe(false);
    });
});
