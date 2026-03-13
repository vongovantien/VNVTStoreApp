import { useState, useEffect } from 'react';
import { useService } from '@/core/di/useService';
import { ServiceKeys } from '@/core/di/ServiceKeys';
import { IPriceHistoryService, PriceHistoryData } from '@/services/product/priceHistory/IPriceHistoryService';

// ============ Hook ============
export const usePriceHistory = (productId: string, currentPrice: number) => {
    const [data, setData] = useState<PriceHistoryData | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<Error | null>(null);

    // DI: Get service instance
    const priceHistoryService = useService<IPriceHistoryService>(ServiceKeys.PriceHistory);

    useEffect(() => {
        if (!productId) return;

        const fetchData = async () => {
            setIsLoading(true);
            try {
                const result = await priceHistoryService.getPriceHistory(productId, currentPrice);
                setData(result);
            } catch (err) {
                setError(err instanceof Error ? err : new Error('Failed to fetch price history'));
            } finally {
                setIsLoading(false);
            }
        };

        fetchData();
    }, [productId, currentPrice, priceHistoryService]);

    return { data, isLoading, error };
};
