export interface PricePoint {
    date: string;
    price: number;
}

export interface PriceHistoryData {
    currentPrice: number;
    lowestPrice: number;
    highestPrice: number;
    averagePrice: number;
    history: PricePoint[];
}

export interface IPriceHistoryService {
    getPriceHistory(productId: string, currentPrice: number): Promise<PriceHistoryData>;
}
