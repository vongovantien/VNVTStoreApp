import axios, { AxiosInstance, AxiosRequestConfig, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { StoreApi } from 'zustand';
import { User } from '@/types';

// Define minimal AuthState interface here to avoid circular dependency with store/index.ts
export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  token: string | null;
  refreshToken: string | null;
  adminToken: string | null; // Used to return from impersonation
  login: (user: User, token?: string, refreshToken?: string, menus?: string[]) => Promise<void>;
  logout: () => void;
  impersonate: (userCode: string) => Promise<void>;
  stopImpersonating: () => void;
  updateUser: (userData: Partial<User>) => void;
  setTokens: (token: string, refreshToken: string) => void;
  permissions: string[];
  setPermissions: (permissions: string[]) => void;
  hasPermission: (permission: string) => boolean;
  menus: string[];
  setMenus: (menus: string[]) => void;
  hasMenu: (menuCode: string) => boolean;
}

let authStore: StoreApi<AuthState> | null = null;
export const injectStore = (store: StoreApi<AuthState>) => {
  authStore = store;
};

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
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export enum SearchCondition {
  Equal = 0,
  NotEqual = 1,
  Contains = 2,
  GreaterThan = 3,
  GreaterThanEqual = 4,
  LessThan = 5,
  LessThanEqual = 6,
  DateTimeRange = 7,
  DayPart = 8,
  MonthPart = 9,
  DatePart = 10,
  IsNull = 11,
  IsNotNull = 12,
  In = 13,
  NotIn = 14,
  EqualExact = 15
}

export interface SearchDTO {
  searchField: string;
  searchCondition: SearchCondition;
  searchValue: string | number | boolean | string[] | number[] | null;
}

export interface SortDTO {
  sortBy: string;
  sort: string; // 'ASC' | 'DESC'
}

export interface RequestDTO {
  pageIndex: number;
  pageSize: number;
  searching?: SearchDTO[];
  sortDTO?: SortDTO;
  fields?: string[];
}

// ============ Discriminated Union Types for Type-Safe State ============

/**
 * Discriminated Union for async state handling
 * Provides type-safe pattern matching for Loading, Success, Error states
 */
export type AsyncState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T }
  | { status: 'error'; error: string };

// Type Guards for AsyncState
export function isIdle<T>(state: AsyncState<T>): state is { status: 'idle' } {
  return state.status === 'idle';
}

export function isLoading<T>(state: AsyncState<T>): state is { status: 'loading' } {
  return state.status === 'loading';
}

export function isSuccess<T>(state: AsyncState<T>): state is { status: 'success'; data: T } {
  return state.status === 'success';
}

export function isError<T>(state: AsyncState<T>): state is { status: 'error'; error: string } {
  return state.status === 'error';
}

// Factory functions for creating AsyncState
export const AsyncState = {
  idle: <T>(): AsyncState<T> => ({ status: 'idle' }),
  loading: <T>(): AsyncState<T> => ({ status: 'loading' }),
  success: <T>(data: T): AsyncState<T> => ({ status: 'success', data }),
  error: <T>(error: string): AsyncState<T> => ({ status: 'error', error }),
  fromApiResponse: <T>(response: ApiResponse<T>): AsyncState<T> =>
    response.success && response.data !== null
      ? { status: 'success', data: response.data }
      : { status: 'error', error: response.message || 'Unknown error' }
};

// ============ Axios Setup ============

const axiosInstance: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000, // 30 seconds timeout
  headers: {
    'Content-Type': 'application/json',
    'Accept-Language': 'vi', // Default to Vietnamese
  },
  withCredentials: true // For XSRF protection if backend supports it
});

// Request Interceptor: Attach Token
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = authStore?.getState()?.token;
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`; // Use standard 'Bearer' prefix
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Variables for 401 Refresh Logic
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: string | null) => void;
  reject: (reason: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response Interceptor: Handle Errors & Refresh Token
axiosInstance.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response) {
      switch (error.response.status) {
        case HttpStatus.UNAUTHORIZED: // 401
          // Special handling for Login/Refresh endpoints to avoid infinite loops
          if (originalRequest.url?.includes('auth/login') || originalRequest.url?.includes('auth/refresh-token')) {
            console.error("Phiên đăng nhập hết hạn hoặc không hợp lệ.");
            authStore?.getState()?.logout();
            return Promise.reject(error);
          }

          if (!originalRequest._retry) {
            if (isRefreshing) {
              return new Promise<string | null>(function (resolve, reject) {
                failedQueue.push({ resolve, reject });
              })
                .then((token) => {
                  if (originalRequest.headers) {
                    originalRequest.headers['Authorization'] = 'Bearer ' + token;
                  }
                  return axiosInstance(originalRequest);
                })
                .catch((err) => {
                  return Promise.reject(err);
                });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
              const state = authStore?.getState();
              if (!state) throw new Error('Store not initialized');
              const { token, refreshToken, setTokens } = state;

              if (!token || !refreshToken) {
                // No tokens to refresh, just logout
                console.error("Token hết hạn, đang chuyển hướng đăng nhập...");
                throw new Error('No tokens available');
              }

              const response = await axios.post<ApiResponse<{ token: string; refreshToken: string }>>(`${API_BASE_URL}/auth/refresh-token`, {
                token,
                refreshToken
              });

              const { data } = response;
              if (data.success && data.data) {
                setTokens(data.data.token, data.data.refreshToken);
                axiosInstance.defaults.headers.common['Authorization'] = 'Bearer ' + data.data.token;
                processQueue(null, data.data.token);

                if (originalRequest.headers) {
                  originalRequest.headers['Authorization'] = 'Bearer ' + data.data.token;
                }
                return axiosInstance(originalRequest);
              } else {
                console.error("Làm mới token thất bại.");
                throw new Error('Refresh failed');
              }
            } catch (err) {
              processQueue(err, null);
              authStore?.getState()?.logout();
              return Promise.reject(err);
            } finally {
              isRefreshing = false;
            }
          }
          break;

        case HttpStatus.FORBIDDEN: // 403
          console.error("Bạn không có quyền truy cập tính năng này.");
          break;

        case HttpStatus.INTERNAL_SERVER_ERROR: // 500
          console.error("Lỗi server, vui lòng thử lại sau.");
          break;

        case HttpStatus.NOT_FOUND: // 404
          console.error("Không tìm thấy tài nguyên yêu cầu.");
          break;

        default:
          console.error(`Đã có lỗi xảy ra (${error.response.status}):`, error.response.data || error.message);
      }
    } else {
      console.error("Lỗi mạng hoặc server không phản hồi.");
    }

    return Promise.reject(error);
  }
);


// ============ API Client Adapter ============
// Maintains compatibility with existing code calling `apiClient.get<T>`
class ApiClient {

  async request<T>(
    method: string,
    endpoint: string,
    data?: unknown,
    options?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    try {
      const response = await axiosInstance.request({
        method,
        url: endpoint,
        data,
        ...options
      });

      const responseData = response.data;

      // Global Validation: Ensure response matches ApiResponse<T> (ResponseDTO) structure
      if (responseData && typeof responseData === 'object' && 'success' in responseData) {
        return responseData as ApiResponse<T>;
      }

      // If response is not a valid DTO (e.g. raw HTML or unexpected JSON), treat as error
      return {
        success: false,
        message: 'Invalid API Response Format', // Or localized message
        data: null,
        statusCode: response.status
      };
    } catch (error: unknown) {
      // Unify error format for frontend consumption
      let msg = 'Unknown error';
      let statusCode: number = HttpStatus.INTERNAL_SERVER_ERROR;

      if (axios.isAxiosError(error)) {
        msg = error.response?.data?.message || (error.message as string);
        statusCode = error.response?.status || HttpStatus.INTERNAL_SERVER_ERROR;
      } else if (error instanceof Error) {
        msg = error.message;
      }

      return {
        success: false,
        message: msg,
        data: null,
        statusCode: statusCode
      };
    }
  }

  get<T>(endpoint: string, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('GET', endpoint, undefined, options);
  }

  post<T>(endpoint: string, data?: unknown, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('POST', endpoint, data, options);
  }

  put<T>(endpoint: string, data?: unknown, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('PUT', endpoint, data, options);
  }

  delete<T>(endpoint: string, options?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>('DELETE', endpoint, undefined, options);
  }

  // Expose axios instance if needed for advanced usage
  get instance() {
    return axiosInstance;
  }
}

export const apiClient = new ApiClient();
export default apiClient;
