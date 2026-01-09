/**
 * API Configuration and Axios Instance
 * Base configuration for calling VNVTStore API
 */

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';

// ============ Types ============
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  statusCode: number;
}

export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface SearchingDTO {
  field: string;
  operator?: string; // eq, ne, contains, gt, lt, gte, lte
  value: string;
}

export interface SortDTO {
  sortBy: string;
  sortDescending: boolean;
}

export interface RequestDTO<T = undefined> {
  pageIndex?: number;
  pageSize?: number;
  searching?: SearchingDTO[];
  sortDTO?: SortDTO;
  postObject?: T;
}

// ============ API Client ============
class ApiClient {
  private baseURL: string;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
  }

  private getAuthHeaders(): Record<string, string> {
    const stored = localStorage.getItem('vnvt-auth');
    if (stored) {
      try {
        const { state } = JSON.parse(stored);
        if (state?.token) {
          return { Authorization: `Bearer ${state.token}` };
        }
      } catch {
        // Ignore parse errors
      }
    }
    return {};
  }

  async request<T>(
    method: string,
    endpoint: string,
    data?: unknown,
    options?: RequestInit
  ): Promise<ApiResponse<T>> {
    const url = `${this.baseURL}${endpoint}`;

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      'Accept-Language': localStorage.getItem('language') || 'vi',
      ...this.getAuthHeaders(),
      ...(options?.headers as Record<string, string>),
    };

    try {
      const response = await fetch(url, {
        method,
        headers,
        body: data ? JSON.stringify(data) : undefined,
        ...options,
      });

      // Handle 204 No Content
      if (response.status === 204) {
        return {
          success: true,
          message: 'Success',
          data: null,
          statusCode: 204
        };
      }

      const result = await response.json();

      // The backend now always returns ApiResponse<T> structure
      return result as ApiResponse<T>;
    } catch (error) {
      console.error('API Error:', error);
      return {
        success: false,
        message: error instanceof Error ? error.message : 'Network error',
        data: null,
        statusCode: 500,
      };
    }
  }

  get<T>(endpoint: string, options?: RequestInit): Promise<ApiResponse<T>> {
    return this.request<T>('GET', endpoint, undefined, options);
  }

  post<T>(endpoint: string, data?: unknown, options?: RequestInit): Promise<ApiResponse<T>> {
    return this.request<T>('POST', endpoint, data, options);
  }

  put<T>(endpoint: string, data?: unknown, options?: RequestInit): Promise<ApiResponse<T>> {
    return this.request<T>('PUT', endpoint, data, options);
  }

  delete<T>(endpoint: string, options?: RequestInit): Promise<ApiResponse<T>> {
    return this.request<T>('DELETE', endpoint, undefined, options);
  }
}

export const apiClient = new ApiClient(API_BASE_URL);
export default apiClient;
