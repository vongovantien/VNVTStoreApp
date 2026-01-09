/**
 * Base Service Factory
 * Creates reusable CRUD services
 * Only these methods should be used by all services
 */

import { apiClient, type ApiResponse, type PagedResult, type RequestDTO } from './api';
import { PageSize, API_ENDPOINTS } from '@/constants';

// Re-export types
export type { ApiResponse, PagedResult, RequestDTO };
export { API_ENDPOINTS, PageSize };

// ============ Types ============
export interface CrudServiceConfig {
    endpoint: string;
    resourceName?: string;
}

export interface SearchParams {
    pageIndex?: number;
    pageSize?: number;
    search?: string;
    searchField?: string;
    sortBy?: string;
    sortDesc?: boolean;
}

// ============ HTTP Methods (internal) ============
const http = {
    get: <T>(url: string) => apiClient.get<T>(url),
    post: <T>(url: string, data?: unknown) => apiClient.post<T>(url, data),
    put: <T>(url: string, data?: unknown) => apiClient.put<T>(url, data),
    delete: <T>(url: string) => apiClient.delete<T>(url),
};

// ============ Base Service Factory ============
export function createCrudService<
    TDto,
    TCreateDto = Partial<TDto>,
    TUpdateDto = Partial<TDto>
>(config: CrudServiceConfig) {
    const { endpoint } = config;

    return {
        /**
         * Search with pagination
         */
        async search(params: SearchParams = {}): Promise<ApiResponse<PagedResult<TDto>>> {
            const request: RequestDTO = {
                pageIndex: params.pageIndex ?? 1,
                pageSize: params.pageSize ?? PageSize.DEFAULT,
                searching: params.search && params.searchField
                    ? [{ field: params.searchField, operator: 'contains', value: params.search }]
                    : undefined,
                sortDTO: params.sortBy
                    ? { sortBy: params.sortBy, sortDescending: params.sortDesc ?? false }
                    : undefined,
            };

            return http.post<PagedResult<TDto>>(`${endpoint}/search`, request);
        },

        /**
         * Get all (with default large page size for dropdowns)
         */
        async getAll(pageSize: number = PageSize.XLARGE): Promise<ApiResponse<PagedResult<TDto>>> {
            return this.search({ pageIndex: 1, pageSize });
        },

        /**
         * Get list without pagination
         */
        async getList(): Promise<ApiResponse<TDto[]>> {
            return http.get<TDto[]>(endpoint);
        },

        /**
         * Get by code
         */
        async getByCode(code: string): Promise<ApiResponse<TDto>> {
            return http.get<TDto>(`${endpoint}/${code}`);
        },

        /**
         * Create new item
         */
        async create(data: TCreateDto): Promise<ApiResponse<TDto>> {
            return http.post<TDto>(endpoint, data);
        },

        /**
         * Update item
         */
        async update(code: string, data: TUpdateDto): Promise<ApiResponse<TDto>> {
            return http.put<TDto>(`${endpoint}/${code}`, data);
        },

        /**
         * Delete item
         */
        async delete(code: string): Promise<ApiResponse<void>> {
            return http.delete<void>(`${endpoint}/${code}`);
        },
    };
}

// ============ Mutation Helper with Toast ============
export function handleApiResponse<T>(
    response: ApiResponse<T>,
    toast: { success: (msg: string) => void; error: (msg: string) => void },
    t: (key: string) => string,
    successKey: string = 'messages.success',
    errorKey: string = 'messages.error'
): boolean {
    if (response.success) {
        toast.success(response.message || t(successKey));
        return true;
    } else {
        toast.error(response.message || t(errorKey));
        return false;
    }
}

export default createCrudService;
