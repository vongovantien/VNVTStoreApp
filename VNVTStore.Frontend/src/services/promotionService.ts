import { ApiResponse } from './api';
import apiClient from './api';
import createEntityService, { SearchParams } from './baseService';

export interface Promotion {
    code: string;
    name: string;
    description?: string;
    discountType: 'PERCENTAGE' | 'AMOUNT' | 'FIXED_PRICE';
    discountValue: number;
    minOrderAmount?: number;
    maxDiscountAmount?: number;
    startDate: string;
    endDate: string;
    usageLimit?: number;
    isActive: boolean;
    productCodes?: string[];
}

export interface CreatePromotionRequest {
    code: string;
    name: string;
    description?: string;
    discountType: string;
    discountValue: number;
    minOrderAmount?: number;
    maxDiscountAmount?: number;
    startDate: string; // ISO string
    endDate: string; // ISO string
    usageLimit?: number;
    isActive: boolean;
    productCodes?: string[];
}

export interface UpdatePromotionRequest extends CreatePromotionRequest { }

export interface PromotionFilter extends SearchParams {
    activeOnly?: boolean;
}

const baseService = createEntityService<Promotion, CreatePromotionRequest, UpdatePromotionRequest>({
    endpoint: '/promotions',
    resourceName: 'Promotion'
});

export const promotionService = {
    ...baseService,

    // Custom override or extension if needed, e.g. getAll with specific filter
    getAll: async (filter: PromotionFilter) => {
        // If baseService.search handles generic params, we can use it.
        // However, our backend specific 'activeOnly' param might not be standard in base search.
        // If baseService.search puts everything in 'params', we can just pass filter if it matches SearchParams structure.
        // PromotionFilter extends SearchParams. 
        // We might need to manually handle 'activeOnly' if baseService doesn't pass extra params query string.
        // Looking at baseService.ts, it constructs a RequestDTO (POST search). 
        // Does the backend /promotions/search endpoint accept generic query params or just RequestDTO in body?
        // The Controller has `[HttpGet]` for Search with [FromQuery].
        // BaseService uses `http.post<PagedResult<TDto>>(`${endpoint}/search`, request);` which is POST.

        // WAIT. The controller I wrote uses GET with [FromQuery].
        // BaseService expects POST to /search.
        // I should probably align the Controller to match BaseService or Override here.
        // Let's override here to use GET as currently implemented in Controller, 
        // OR change Controller to match BaseService pattern (POST search).
        // Given the user wants "extend base", matching BaseService pattern is better.
        // I will explicitly use apiClient for this specific method if the pattern differs,
        // OR likely logic: BaseService is the "Standard", so Backend should match it.

        // BUT, for now, let's keep the specific GET endpoint usage or use the BaseService.search which does POST.
        // If I use baseService.search, it sends a body.
        // My Controller `Search` is GET. 
        // I will Rename Controller `Search` to `SearchExtensions` or just support POST search in Controller?
        // Actually, `BaseApiController` often has a `Search` POST endpoint? 
        // Let's check `ProductsController`. It has `[HttpPost("search")]`.
        // So my `PromotionsController` should also use Post Search if I want to use BaseService.

        // I will change `PromotionsController` to use `[HttpPost("search")]` later to align.
        // For now, I'll impl a custom `getPromotions` that matches the current GET controller.

        const response = await apiClient.get<ApiResponse<any>>('/promotions', { params: filter });
        return response.data;
    },

    getFlashSales: async () => {
        const response = await apiClient.get<ApiResponse<Promotion[]>>('/promotions/flash-sale');
        return response.data;
    }
};
