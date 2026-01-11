import { useAuthStore } from '@/store';

/**
 * API Configuration and Axios Instance
 * Base configuration for calling VNVTStore API
 */

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';

// ============ Constants ============
export const HttpStatus = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
} as const;

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
}

export enum SearchCondition {
  Equal = 0,
  Contains = 1,
  GreaterThan = 2,
  LessThan = 3,
  GreaterThanOrEqual = 4,
  LessThanOrEqual = 5,
  NotEqual = 6,
  IsNull = 7,
  IsNotNull = 8
}

export interface SearchDTO {
  field: string;
  operator: SearchCondition;
  value: string;
}

export interface SortDTO {
  sortBy: string;
  sortDescending: boolean;
}

export interface RequestDTO {
  pageIndex: number;
  pageSize: number;
  searching?: SearchDTO[];
  sortDTO?: SortDTO;
}

// ============ API Client ============
class ApiClient {
  private baseURL: string;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
  }

  private getAuthHeaders(): Record<string, string> {
    const { token } = useAuthStore.getState();
    if (token) {
      return { Authorization: `Bearer ${token}` };
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
      ...this.getAuthHeaders(),
      ...(options?.headers as Record<string, string>),
    };

    try {
      let response = await fetch(url, {
        method,
        headers,
        body: data ? JSON.stringify(data) : undefined,
        ...options,
      });

      // Handle 401 Unauthorized - Refresh Token Logic
      if (response.status === HttpStatus.UNAUTHORIZED) {
        // Avoid infinite loop if refresh token endpoint itself returns 401
        if (endpoint.includes('refresh-token') || endpoint.includes('login')) {
          useAuthStore.getState().logout();
          return {
            success: false,
            message: 'Session expired',
            data: null,
            statusCode: HttpStatus.UNAUTHORIZED
          };
        }

        const success = await this.handleRefreshToken();
        if (success) {
          // Retry original request with new token
          headers['Authorization'] = `Bearer ${useAuthStore.getState().token}`;
          response = await fetch(url, {
            method,
            headers,
            body: data ? JSON.stringify(data) : undefined,
            ...options,
          });
        } else {
          useAuthStore.getState().logout();
          return {
            success: false,
            message: 'Session expired',
            data: null,
            statusCode: HttpStatus.UNAUTHORIZED
          };
        }
      }

      // The backend now always returns ApiResponse<T> structure including deletes
      const result = await response.json();
      return result as ApiResponse<T>;
    } catch (error) {
      console.error('API Error:', error);
      return {
        success: false,
        message: error instanceof Error ? error.message : 'Network error',
        data: null,
        statusCode: HttpStatus.INTERNAL_SERVER_ERROR,
      };
    }
  }

  private async handleRefreshToken(): Promise<boolean> {
    const { token, refreshToken, setTokens } = useAuthStore.getState();
    if (!token || !refreshToken) return false;

    try {
      // Call direct fetch to avoid circular dependency or infinite loop
      const response = await fetch(`${this.baseURL}/auth/refresh-token`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ token, refreshToken })
      });

      if (response.ok) {
        const result = await response.json();
        if (result.success && result.data) {
          setTokens(result.data.token, result.data.refreshToken);
          return true;
        }
      }
    } catch (error) {
      console.error("Refresh token failed", error);
    }
    return false;
  }

  // Helper methods...

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
