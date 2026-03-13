import { IPriceHistoryService, PriceHistoryData } from './IPriceHistoryService';

export class PriceHistoryService implements IPriceHistoryService {
    async getPriceHistory(productId: string, currentPrice: number): Promise<PriceHistoryData> {
        console.debug(`Generating simulated price history for product: ${productId}`);
        // Simulated historical data since backend doesn't have it yet
        const now = new Date();
        const history = Array.from({ length: 7 }, (_, i) => {
            const date = new Date(now);
            date.setDate(now.getDate() - (6 - i));

            // Generate some realistic fluctuation around current price
            const fluctuation = (Math.random() - 0.5) * currentPrice * 0.1;
            return {
                date: date.toISOString().split('T')[0],
                price: Math.round(currentPrice + fluctuation)
            };
        });

        const prices = history.map(h => h.price);

        return {
            currentPrice,
            lowestPrice: Math.min(...prices),
            highestPrice: Math.max(...prices),
            averagePrice: Math.round(prices.reduce((a, b) => a + b, 0) / prices.length),
            history
        };
    }
}
