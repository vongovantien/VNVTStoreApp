/**
 * Base Service Factory
 * Creates reusable CRUD services
 * Only these methods should be used by all services
 */

import { apiClient, type ApiResponse, type PagedResult, type RequestDTO, SearchCondition } from './api';
import { PageSize, API_ENDPOINTS } from '@/constants';

// Re-export types
export type { ApiResponse, PagedResult, RequestDTO };
export { API_ENDPOINTS, PageSize, SearchCondition };

// ============ Types ============
export interface EntityServiceConfig {
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
    filters?: Array<{ field: string; value: string; operator?: SearchCondition }>;
}

// ============ HTTP Methods (internal) ============
const http = {
    get: <T>(url: string) => apiClient.get<T>(url),
    post: <T>(url: string, data?: unknown) => apiClient.post<T>(url, data),
    put: <T>(url: string, data?: unknown) => apiClient.put<T>(url, data),
    delete: <T>(url: string) => apiClient.delete<T>(url),
};

// ============ Base Entity Service Factory ============
export function createEntityService<
    TDto, // Assuming BaseDto is not needed as it's not defined in the original file. If it were, it would need to be imported/defined.
    TCreateDto = Partial<TDto>,
    TUpdateDto = Partial<TDto>
>(config: EntityServiceConfig) {
    const { endpoint } = config;

    return {
        /**
         * Search with pagination
         */
        async search(params: SearchParams = {}): Promise<ApiResponse<PagedResult<TDto>>> {
            const searching = [];

            if (params.search && params.searchField) {
                searching.push({ field: params.searchField, operator: SearchCondition.Contains, value: params.search });
            }

            if (params.filters) {
                searching.push(...params.filters.map(f => ({
                    field: f.field,
                    operator: f.operator ?? SearchCondition.Equal,
                    value: f.value
                })));
            }

            const request: RequestDTO = {
                pageIndex: params.pageIndex ?? 1,
                pageSize: params.pageSize ?? PageSize.DEFAULT,
                searching: searching.length > 0 ? searching : undefined,
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
            const response = await http.post<TDto>(endpoint, { PostObject: data });
            if (!response.success) {
                throw new Error(response.message || 'Create failed');
            }
            return response;
        },

        /**
         * Update item
         */
        async update(code: string, data: TUpdateDto): Promise<ApiResponse<TDto>> {
            const response = await http.put<TDto>(`${endpoint}/${code}`, { PostObject: data });
            if (!response.success) {
                throw new Error(response.message || 'Update failed');
            }
            return response;
        },

        /**
         * Delete item
         */
        async delete(code: string): Promise<ApiResponse<void>> {
            const response = await http.delete<void>(`${endpoint}/${code}`);
            if (!response.success) {
                throw new Error(response.message || 'Delete failed');
            }
            return response;
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

export default createEntityService;
